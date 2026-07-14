// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapByPoints : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e61f2f2e-f655-430a-9dfd-507929edef58");

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
        public SAMAnalyticalSnapByPoints()
          : base("SAMAnalytical.SnapByPoints", "SAMAnalytical.SnapByPoints",
              "Snap Panels to Points",
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

                result.Add(new GH_SAMParam(new Geometry.Grasshopper.GooSAMGeometryParam() { Name = "_points", NickName = "_points", Description = "List of Points", Access = GH_ParamAccess.list, DataMapping = GH_DataMapping.Flatten }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxDistance_", NickName = "_maxDistance_", Description = "Max Distance to snap points default 1m", Access = GH_ParamAccess.item };
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

            index = Params.IndexOfInputParam("_points");
            List<Geometry.ISAMGeometry> geometries = new List<Geometry.ISAMGeometry>();
            if (index == -1 || !dataAccess.GetDataList(index, geometries) || geometries == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_maxDistance_");
            double maxDistance = double.NaN;
            if (index == -1 || !dataAccess.GetData(index, ref maxDistance) || double.IsNaN(maxDistance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Geometry.Spatial.Point3D> point3Ds = geometries?.Cast<Geometry.Spatial.Point3D>()?.ToList();
            Geometry.Spatial.Modify.RemoveAlmostSimilar(point3Ds);

            Panel result = Create.Panel(panel);
            result.Snap(point3Ds, maxDistance);

            index = Params.IndexOfOutputParam("_Panel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooPanel(result));
            }
        }
    }
}
