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
    public class SAMGeometryPlaneIntersection : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("02c2bed1-ef27-48be-bcf0-dc969d0b6d90");

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
        public SAMGeometryPlaneIntersection()
          : base("SAMGeometry.PlaneIntersection", "GHgeo",
              "Plane Intersection",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometry3D", NickName = "_geometry3D", Description = "SAM Geometry 3D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_plane", NickName = "_plane", Description = "SAM Plane", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "Geometries", NickName = "Geometries", Description = "Intersection Geometries", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index_Successful = Params.IndexOfOutputParam("Successful");
            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            int index = -1;

            GH_ObjectWrapper objectWrapper = null;

            index = Params.IndexOfInputParam("_geometry3D");
            objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object obj = objectWrapper.Value;
            if (obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> geometry3Ds = null;

            if (obj is IGH_GeometricGoo)
                geometry3Ds = Convert.ToSAM((IGH_GeometricGoo)obj);
            else if (obj is ISAMGeometry3D)
                geometry3Ds = new List<ISAMGeometry3D>() { (ISAMGeometry3D)obj };
            else if (obj is GooSAMGeometry)
                geometry3Ds = new List<ISAMGeometry3D>() { ((GooSAMGeometry)obj).Value as ISAMGeometry3D };

            index = Params.IndexOfInputParam("_plane");
            objectWrapper = null;
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Plane plane = null;

            if (objectWrapper.Value is Plane)
                plane = objectWrapper.Value as Plane;
            else if (objectWrapper.Value is GooSAMGeometry)
                plane = ((GooSAMGeometry)objectWrapper.Value).Value as Plane;
            else if (objectWrapper.Value is GH_Plane)
                plane = ((GH_Plane)objectWrapper.Value).ToSAM();

            if (plane == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMGeometry3D> result_Geometries = new List<ISAMGeometry3D>();
            foreach (ISAMGeometry3D geometry3D in geometry3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = Spatial.Create.PlanarIntersectionResult(plane, geometry3D as dynamic);
                if (planarIntersectionResult == null)
                    continue;

                List<ISAMGeometry3D> geometry3Ds_Temp = planarIntersectionResult.Geometry3Ds;
                if (geometry3Ds_Temp != null && geometry3Ds_Temp.Count > 0)
                    result_Geometries.AddRange(geometry3Ds_Temp);
            }

            index = Params.IndexOfOutputParam("Geometries");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result_Geometries.ConvertAll(x => new GooSAMGeometry(x)));
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, true);
            }
        }
    }
}
