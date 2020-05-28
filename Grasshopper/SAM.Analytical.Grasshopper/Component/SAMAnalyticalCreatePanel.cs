using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanel : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("35ef8f3e-1cf2-407d-b2ed-33bf371ea161");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreatePanel()
          : base("SAMAnalytical.CreatePanel", "SAMAnalytical.CreatePanel",
              "Create SAM Analytical Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddGenericParameter("_geometry", "_geometry", "Geometry", GH_ParamAccess.item);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            index = inputParamManager.AddGenericParameter("panelType_", "panelType_", "PanelType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            //GooConstructionParam gooConstructionParam = new GooConstructionParam();
            //gooConstructionParam.PersistentData.Append(new GooConstruction(Query.Construction(PanelType.Roof)));
            index = inputParamManager.AddParameter(new GooConstructionParam(), "construction_", "construction_", "SAM Analytical Construction", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("simplify_", "simplify_", "Simplify", GH_ParamAccess.item, true);
            inputParamManager.AddNumberParameter("minArea_", "minArea_", "Minimal Acceptable area of Aperture", GH_ParamAccess.item, Core.Tolerance.MacroDistance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool simplyfy = false;
            if (!dataAccess.GetData<bool>(3, ref simplyfy))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = null;
            if (!dataAccess.GetData(0, ref @object))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = null;
            if (@object is IGH_GeometricGoo)
            {
                geometry3Ds = ((IGH_GeometricGoo)@object).ToSAM(simplyfy).Cast<ISAMGeometry3D>().ToList();
            }
            else if (@object is GH_ObjectWrapper)
            {
                GH_ObjectWrapper objectWrapper_Temp = ((GH_ObjectWrapper)@object);
                if (objectWrapper_Temp.Value is ISAMGeometry3D)
                    geometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)objectWrapper_Temp.Value };
            }
            else if (@object is IGH_Goo)
            {
                Geometry.ISAMGeometry sAMGeometry = (@object as dynamic).Value as Geometry.ISAMGeometry;
                if (sAMGeometry is ISAMGeometry3D)
                    geometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)sAMGeometry };
                else if (sAMGeometry is Geometry.Planar.ISAMGeometry2D)
                    geometry3Ds = new List<ISAMGeometry3D>() { Plane.WorldXY.Convert(sAMGeometry as dynamic) };
            }

            if (geometry3Ds == null || geometry3Ds.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid data");
                return;
            }

            PanelType panelType = PanelType.Undefined;

            GH_ObjectWrapper objectWrapper = null;
            dataAccess.GetData(1, ref objectWrapper);
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    panelType = Analytical.Query.PanelType(((GH_String)objectWrapper.Value).Value);
                else
                    panelType = Analytical.Query.PanelType(objectWrapper.Value);
            }

            Construction construction = null;
            dataAccess.GetData(2, ref construction);

            double minArea = Core.Tolerance.MacroDistance;
            dataAccess.GetData(4, ref minArea);

            List<Panel> panels = Create.Panels(geometry3Ds, panelType, construction, minArea);
            if (panels == null || panels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid geometry for panel");
                return;
            }

            if (panels.Count == 1)
                dataAccess.SetData(0, new GooPanel(panels[0]));
            else
                dataAccess.SetDataList(0, panels.ConvertAll(x => new GooPanel(x)));
        }
    }
}