// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalExtendPanelByPlane : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5a3ad285-3029-4b7d-9087-a08c9699c8c3");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalExtendPanelByPlane()
          : base("SAMAnalytical.ExtendPanelByPlane", "SAMAnalytical.ExtendPanelByPlane",
              "Extend SAM Analytical Panel By Given Plane",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Plane() { Name = "_plane", NickName = "_plane", Description = "Plane", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(Core.Tolerance.Distance);
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfInputParam("_panels");
            List<Panel> panels = new List<Panel>();
            if (index == -1 || !dataAccess.GetDataList(index, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_plane");
            Plane plane_Rhino = Plane.Unset;
            if (index == -1 || !dataAccess.GetData(index, ref plane_Rhino) || plane_Rhino == Plane.Unset)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Plane plane = Geometry.Rhino.Convert.ToSAM(plane_Rhino);

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> result = panels.ConvertAll(x => Analytical.Query.Extend(x, plane, Core.Tolerance.MacroDistance, tolerance));

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result?.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
