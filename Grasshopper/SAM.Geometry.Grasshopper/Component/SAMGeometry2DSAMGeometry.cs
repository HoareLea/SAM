// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometry2DSAMGeometry : GH_SAMVariableOutputParameterComponent
    {

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7d24437d-df27-4144-b66e-82d8a8491b2d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMGeometry2DSAMGeometry()
          : base("SAMGeometry2D.SAMGeometry", "SAMGeometry2D.SAMGeometry",
              "Convert SAM geometry 2D to SAM geometry 3D",
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

                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "_SAMGeometry2D", NickName = "SAMgeo2D", Description = "SAM Geometry 2D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                GooSAMGeometryParam gooSAMGeometryParam = new GooSAMGeometryParam() { Name = "Plane", NickName = "Plane", Description = "SAM Plane", Access = GH_ParamAccess.item };
                gooSAMGeometryParam.SetPersistentData(new GooSAMGeometry(Plane.WorldXY));
                result.Add(new GH_SAMParam(gooSAMGeometryParam, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "SAMGeometry3D", NickName = "SAMgeo3D", Description = "SAM Geometry 3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_SAMGeometry2D");
            ISAMGeometry sAMGeometry = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMGeometry) || sAMGeometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Planar.ISAMGeometry2D geometry2D = sAMGeometry as Planar.ISAMGeometry2D;
            if (geometry2D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            sAMGeometry = null;
            index = Params.IndexOfInputParam("Plane");
            if (index == -1 || !dataAccess.GetData(index, ref sAMGeometry) || sAMGeometry == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane = sAMGeometry as Plane;
            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ISAMGeometry3D geometry3D = plane.Convert(geometry2D);
            if (geometry3D == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
                return;
            }

            index = Params.IndexOfOutputParam("SAMGeometry3D");
            if (index != -1)
            {
                dataAccess.SetData(index, geometry3D);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SAM_Geometry;
            }
        }

    }
}
