using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanelByPanels : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("be3ca54c-0795-4b75-b277-9c863481e3e6");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreatePanelByPanels()
          : base("SAMAnalytical.CreatePanelByPanels", "SAMAnalytical.CreatePanelByPanels",
              "New Panel will be generated from provided Panels and Plane or Elevation., ie Floor from Wall Panels",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_panels", "_panels", "SAM Analytical Panels", GH_ParamAccess.list);
            inputParamManager.AddGenericParameter("_elevation", "_elevation", "Elevation or Plane", GH_ParamAccess.item);

            int index = inputParamManager.AddGenericParameter("panelType_", "panelType_", "PanelType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "New Created Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "UpperPanels", "UpperPanels", "Upper SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "LowerPanels", "LowerPanels", "Lower SAM Analytical Panels", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(1, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            object @object = objectWrapper.Value;

            if (@object is IGH_Goo)
            {
                try
                {
                    @object = (@object as dynamic).Value;
                }
                catch (Exception exception)
                {
                    @object = null;
                }
            }

            if (@object == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            Plane plane = null;

            if (Core.Query.IsNumeric(@object))
            {
                plane = Geometry.Spatial.Create.Plane((double)@object);
            }
            else if (@object is Plane)
            {
                plane = (Plane)@object;
            }
            else if (@object is Rhino.Geometry.Plane)
            {
                plane = ((Rhino.Geometry.Plane)@object).ToSAM();
            }
            else if (@object is GH_Plane)
            {
                plane = ((GH_Plane)@object).ToSAM();
            }
            else if (@object is string)
            {
                double value;
                if (double.TryParse((string)@object, out value))
                    plane = Geometry.Spatial.Create.Plane(value);
            }

            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = PanelType.Undefined;

            objectWrapper = null;
            dataAccess.GetData(2, ref objectWrapper);
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    panelType = Analytical.Query.PanelType(((GH_String)objectWrapper.Value).Value);
                else
                    panelType = Analytical.Query.PanelType(objectWrapper.Value);
            }

            List<Panel> panels_Result = Create.Panels(panels, plane, panelType, true);

            List<Panel> panels_Upper = new List<Panel>();
            List<Panel> panels_Lower = new List<Panel>();

            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                List<Panel> panels_Cut = Analytical.Query.Cut(Create.Panel(panel), plane);
                if (panels_Cut == null)
                    panels_Cut = new List<Panel>();

                if (panels_Cut.Count == 0)
                    panels_Cut.Add(panel);

                foreach(Panel panel_Cut in panels_Cut)
                {
                    Point3D point3D = panel_Cut.GetInternalPoint3D();

                    if (plane.Above(point3D) || plane.On(point3D))
                        panels_Upper.Add(panel_Cut);
                    else
                        panels_Lower.Add(panel_Cut);

                }
            }

            dataAccess.SetDataList(0, panels_Result?.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(1, panels_Upper.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(2, panels_Lower.ConvertAll(x => new GooPanel(x)));
        }
    }
}