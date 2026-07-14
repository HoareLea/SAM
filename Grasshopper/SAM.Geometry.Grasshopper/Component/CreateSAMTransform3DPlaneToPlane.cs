// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class CreateSAMTransform3DPlaneToPlane : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("691344f1-4b41-469f-8bf2-36c46450c611");

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
        public CreateSAMTransform3DPlaneToPlane()
          : base("Create.SAMTransform3DPlaneToPlane", "Create.Transoform3DPlaneToPlane",
              "Creates SAM Transform3D Plane To Plane)",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_source", NickName = "_source", Description = "Source Rhino or SAM Plane", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_target", NickName = "_target", Description = "Target Rhino or SAM Plane", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooTransform3DParam() { Name = "Transform3D", NickName = "Transform3D", Description = "SAM Geometry Transform3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_source");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Query.TryGetSAMGeometries(objectWrapper, out List<Plane> planes_1) || planes_1 == null || planes_1.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane_1 = planes_1[0];
            if (plane_1 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            objectWrapper = null;
            index = Params.IndexOfInputParam("_target");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!Query.TryGetSAMGeometries(objectWrapper, out List<Plane> planes_2) || planes_2 == null || planes_2.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane_2 = planes_2[0];
            if (plane_2 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("Transform3D");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooTransform3D(Transform3D.GetPlaneToPlane(plane_1, plane_2)));
            }
        }
    }
}
