// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterByMaxRectangle3D : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e87bbf9e-b61f-4dde-9232-06ae944494bd");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalFilterByMaxRectangle3D()
          : base("SAMAnalytical.FilterByMaxRectangle3D", "SAMAnalytical.FilterByMaxRectangle3D",
              "Filter Analytical Objects By Maximal Rectangle3D dimension",
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panels", NickName = "_panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_panelMinDimension", NickName = "_panelMinDimension", Description = "Minimal dimension for Panel Rectangle3D", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_apertureMinDimension", NickName = "_apertureMinDimension", Description = "Minimal dimension for Aperture Rectangle3D", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "in", NickName = "in", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "out", NickName = "out", Description = "out", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_panelMinDimension");
            double panelMinDimension = 0;
            if (index == -1 || !dataAccess.GetData(index, ref panelMinDimension))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_apertureMinDimension");
            double apertureMinDimension = 0;
            if (index == -1 || !dataAccess.GetData(index, ref apertureMinDimension))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels_In = new List<Panel>();
            List<object> objects_Out = new List<object>();

            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = Create.Panel(panels[i]);
                if (panel == null)
                {
                    continue;
                }

                Geometry.Spatial.Rectangle3D rectangle3D = Geometry.Object.Spatial.Query.MaxRectangle3D(panel);
                if (rectangle3D == null || rectangle3D.Width < panelMinDimension || rectangle3D.Height < panelMinDimension)
                {
                    objects_Out.Add(panel);
                }
                else
                {
                    panels_In.Add(panel);
                }

                List<Aperture> apertures = panel.Apertures;
                if (apertures == null || apertures.Count == 0)
                {
                    continue;
                }

                foreach (Aperture aperture in apertures)
                {
                    rectangle3D = Geometry.Object.Spatial.Query.MaxRectangle3D(aperture);
                    if (rectangle3D == null || rectangle3D.Width < apertureMinDimension || rectangle3D.Height < apertureMinDimension)
                    {
                        objects_Out.Add(aperture);
                        panel.RemoveAperture(aperture.Guid);
                    }
                }
            }

            index = Params.IndexOfOutputParam("in");
            if (index != -1)
            {
                dataAccess.SetDataList(index, panels_In?.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("out");
            if (index != -1)
            {
                dataAccess.SetDataList(index, objects_Out);
            }
        }
    }
}
