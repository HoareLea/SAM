// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanelByPanels : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("be3ca54c-0795-4b75-b277-9c863481e3e6");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreatePanelByPanels()
          : base("SAMAnalytical.CreatePanelByPanels", "SAMAnalytical.CreatePanelByPanels",
              "New Panel will be generated from provided Panels and Plane or Elevation., ie Floor from Wall Panels",
              "SAM", "Analytical01")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_elevation", NickName = "_elevation", Description = "Elevation or Plane", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "panelType_", NickName = "panelType_", Description = "PanelType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "New Created Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "UpperPanels", NickName = "UpperPanels", Description = "Upper SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "LowerPanels", NickName = "LowerPanels", Description = "Lower SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels) || panels == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            index = Params.IndexOfInputParam("_elevation");
            GH_ObjectWrapper objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
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
                catch (Exception)
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
            else if (@object is global::Rhino.Geometry.Plane)
            {
                plane = Geometry.Rhino.Convert.ToSAM((global::Rhino.Geometry.Plane)@object);
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
            else if (@object is Architectural.Level)
            {
                plane = Geometry.Spatial.Create.Plane(((Architectural.Level)@object).Elevation);
            }

            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = PanelType.Undefined;

            objectWrapper = null;
            index = Params.IndexOfInputParam("panelType_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
            }
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    panelType = Analytical.Query.PanelType(((GH_String)objectWrapper.Value).Value);
                else
                    panelType = Analytical.Query.PanelType(objectWrapper.Value);
            }

            List<Panel> panels_Result = Create.Panels(panels, plane, panelType, true, false);

            List<Panel> panels_Upper = new List<Panel>();
            List<Panel> panels_Lower = new List<Panel>();

            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                List<Panel> panels_Cut = Analytical.Query.Cut(Create.Panel(panel), plane);
                if (panels_Cut == null)
                    panels_Cut = new List<Panel>();

                if (panels_Cut.Count == 0)
                    panels_Cut.Add(panel);

                foreach (Panel panel_Cut in panels_Cut)
                {
                    Point3D point3D = panel_Cut.GetInternalPoint3D();

                    if (plane.Above(point3D) || plane.On(point3D))
                        panels_Upper.Add(panel_Cut);
                    else
                        panels_Lower.Add(panel_Cut);

                }
            }

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_Result?.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("UpperPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_Upper.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("LowerPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_Lower.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
