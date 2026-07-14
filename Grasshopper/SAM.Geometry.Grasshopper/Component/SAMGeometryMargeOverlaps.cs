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
    public class SAMGeometryMergeOverlaps : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2b27fa9e-6ceb-45e5-a99c-b6d057173d5c");

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
        public SAMGeometryMergeOverlaps()
          : base("SAMGeometry.MergeOverlaps", "SAMGeometry.MergeOverlaps",
              "Merge Face3Ds Overlaps",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_face3Ds", NickName = "_face3Ds", Description = "SAM Geometry Face3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "face3Ds", NickName = "face3Ds", Description = "SAM Geometry Face3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Successful", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, false);
            }

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();

            index = Params.IndexOfInputParam("_face3Ds");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if (Query.TryGetSAMGeometries(objectWrapper, out List<Face3D> face3Ds_Temp))
                {
                    face3Ds.AddRange(face3Ds_Temp);
                }
            }

            if (face3Ds != null && face3Ds.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (face3Ds.Count > 1)
            {
                Plane plane = face3Ds[0].GetPlane();

                List<Planar.Face2D> face2Ds = face3Ds.ConvertAll(x => plane.Convert(plane.Project(x)));

                Planar.Modify.MergeOverlaps(face2Ds);

                face3Ds = face2Ds.ConvertAll(x => plane.Convert(x));
            }

            index = Params.IndexOfOutputParam("face3Ds");
            if (index != -1)
            {
                dataAccess.SetDataList(index, face3Ds.ConvertAll(x => new GooSAMGeometry(x)));
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, true);
            }
        }
    }
}
