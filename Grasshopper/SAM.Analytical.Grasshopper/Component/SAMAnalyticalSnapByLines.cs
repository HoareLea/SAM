// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByLines : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("32682bee-68b4-4de0-ab72-6b01233a9764");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// ` Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapByLines()
          : base("SAMAnalytical.SnapByLines", "SAMAnalytical.SnapByLines",
              "Snap Panels to Lines",
              "SAM", "Analytical03")
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_Panel", NickName = "_Panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Line() { Name = "_lines", NickName = "_lines", Description = "Lines", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxDistance_", NickName = "_maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(1);
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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_Panel", NickName = "_Panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_Panel");
            Panel panel = null;
            if (index == -1 || !dataAccess.GetData(index, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_lines");
            List<Line> lines = new List<Line>();
            if (index == -1 || !dataAccess.GetDataList(index, lines) || lines == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.WorldXY;
            List<Geometry.Spatial.Plane> planes = new List<Geometry.Spatial.Plane>();

            foreach (Segment3D segment3D in lines.ConvertAll(x => Geometry.Rhino.Convert.ToSAM(x)))
            {
                Geometry.Spatial.Plane plane_Segment3D = new Geometry.Spatial.Plane(segment3D[0], segment3D[1], (Point3D)segment3D[0].GetMoved(plane.Normal));
                if (plane_Segment3D != null)
                    planes.Add(plane_Segment3D);
            }

            index = Params.IndexOfInputParam("_maxDistance_");
            double maxDistance = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref maxDistance) || double.IsNaN(maxDistance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Panel result = Create.Panel(panel);
            result.Snap(planes, maxDistance);

            index = Params.IndexOfOutputParam("_Panel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooPanel(result));
            }
        }
    }
}
