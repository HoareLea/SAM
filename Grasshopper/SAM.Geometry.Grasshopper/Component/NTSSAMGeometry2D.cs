// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class NTSSAMGeometry2D : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("93610b4d-c36a-4294-9290-27bc4c9afca2");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public NTSSAMGeometry2D()
          : base("NTS.SAMGeometry2D", "NTS.SAMGeometry2D",
              "NetTopologySuite To SAM Geometry",
              "SAM", "Geometry")
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_NTS", NickName = "NTS", Description = "NTS Text", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "Geometry2Ds", NickName = "Geometry2Ds", Description = "SAM Geometry 2D", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("_NTS");
            List<string> lines_NTS = new List<string>();
            if (index == -1 || !dataAccess.GetDataList(index, lines_NTS))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry2D> geometry2Ds = Planar.Convert.ToSAM<ISAMGeometry2D>(lines_NTS);

            index = Params.IndexOfOutputParam("Geometry2Ds");
            if (index != -1)
            {
                if (geometry2Ds == null)
                    dataAccess.SetDataList(index, null);
                else
                    dataAccess.SetDataList(index, geometry2Ds.ConvertAll(x => new GooSAMGeometry(x)));
            }
        }
    }
}
