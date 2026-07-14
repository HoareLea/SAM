// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometryTransform : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ca12bdd8-d050-4bf1-9c37-963cfee655f3");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometryTransform()
          : base("SAMGeometry.Transform", "SAMGeometry.Transform",
              "Transforms SAM Geometry",
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "_SAMGeometry", NickName = "_SAMGeometry", Description = "SAM Geometry", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooTransform3DParam() { Name = "_transform3D", NickName = "_transform3D", Description = "SAM Transform 3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "SAMGeometry", NickName = "SAMGeometry", Description = "SAM Geometry", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_SAMGeometry");
            ISAMGeometry geometry = null;
            if (index == -1 || !dataAccess.GetData(index, ref geometry) || geometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_transform3D");
            Spatial.Transform3D transform3D = null;
            if (index == -1 || !dataAccess.GetData(index, ref transform3D) || transform3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            geometry = Spatial.Query.Transform(geometry as dynamic, transform3D);
            if (geometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Geometry could not be transformed");
                return;
            }

            index = Params.IndexOfOutputParam("SAMGeometry");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooSAMGeometry(geometry));
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.SAM_Geometry;
            }
        }
    }
}
