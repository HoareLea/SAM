// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometryMeshReduce : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("50f45468-60e1-4a11-9c2c-0e2b01f72952");

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
        public GeometryMeshReduce()
          : base("Geometry.MeshReduce", "Geometry.MeshReduce",
              "Reduce Rhino Mesh",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Mesh() { Name = "_mesh", NickName = "_mesh", Description = "Rhino Mesh", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_distortion_", NickName = "_distortion_", Description = "Allow Distortion towards desire count", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer;

                param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_count_", NickName = "_count_", Description = "Desired Polygon Count", Access = GH_ParamAccess.item, Optional = true };
                param_Integer.SetPersistentData(44);
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Binding));

                param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_accuracy_", NickName = "_accuracy_", Description = "Accuracy lowest 1 to 10 highest", Access = GH_ParamAccess.item, Optional = true };
                param_Integer.SetPersistentData(10);
                result.Add(new GH_SAMParam(param_Integer, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_normalizedSize_", NickName = "_normalizedSize_", Description = "Normalized Face Size", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Mesh() { Name = "Mesh", NickName = "Mesh", Description = "Mesh", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_mesh");
            Mesh mesh = null;
            if (index == -1 || !dataAccess.GetData(index, ref mesh) || mesh == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool allowDistortion = true;
            index = Params.IndexOfInputParam("_distortion_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref allowDistortion);
            }

            int desiredPolygonCount = 0;
            index = Params.IndexOfInputParam("_count_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref desiredPolygonCount);
            }

            int accuracy = 0;
            index = Params.IndexOfInputParam("_accuracy_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref accuracy);
            }

            bool normalizedSize = true;
            index = Params.IndexOfInputParam("_normalizedSize_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref normalizedSize);
            }

            Modify.Reduce(mesh, allowDistortion, desiredPolygonCount, accuracy, normalizedSize);

            index = Params.IndexOfOutputParam("Mesh");
            if (index != -1)
            {
                dataAccess.SetData(index, mesh);
            }
        }
    }
}
