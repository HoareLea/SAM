// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class GeometrySnapByPoints : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1165eb14-9f8a-460e-a3e7-e9b3d7f08ddc");

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
        public GeometrySnapByPoints()
          : base("Geometry.SnapByPoints", "GeometryEngine.SnapByPoints",
              "Snap Geometry or SAM Geometry by points",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_geometry", NickName = "Geo", Description = "Geometry", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_points", NickName = "Points", Description = "snapping Points", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxDistance_", NickName = "mDis", Description = "Max Distance to snap Geometry points", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(1);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Geometry", NickName = "Geo", Description = "modified SAM Geometry", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Correctly imported?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }
            if (!run)
                return;

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();

            index = Params.IndexOfInputParam("_points");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Spatial.Point3D> point3Ds = new List<Spatial.Point3D>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrappers)
            {
                Spatial.Point3D point3D = objectWrapper_Temp.Value as Spatial.Point3D;
                if (point3D != null)
                {
                    point3Ds.Add(point3D);
                    continue;
                }

                GH_Point point = objectWrapper_Temp.Value as GH_Point;
                if (point == null)
                    continue;

                point3Ds.Add(Convert.ToSAM(point));
            }

            GH_ObjectWrapper objectWrapper = null;

            index = Params.IndexOfInputParam("_maxDistance_");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxDistance = double.NaN;

            GH_Number number = objectWrapper.Value as GH_Number;
            if (number != null)
                maxDistance = number.Value;

            objectWrapper = null;
            index = Params.IndexOfInputParam("_geometry");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object @object = objectWrapper.Value;
            if (@object == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Spatial.ISAMGeometry3D> geometry3Ds = null;

            if (@object is IGH_GeometricGoo)
                geometry3Ds = Convert.ToSAM((IGH_GeometricGoo)@object);
            else if (@object is Spatial.ISAMGeometry3D)
                geometry3Ds = new List<Spatial.ISAMGeometry3D>() { (Spatial.ISAMGeometry3D)@object };

            for (int i = 0; i < geometry3Ds.Count; i++)
            {
                if (geometry3Ds[i] is Spatial.Point3D)
                {
                    geometry3Ds[i] = Spatial.Query.Snap(point3Ds, (Spatial.Point3D)geometry3Ds[i], maxDistance);
                }
                else if (geometry3Ds[i] is Spatial.Segment3D)
                {
                    geometry3Ds[i] = Spatial.Query.Snap(point3Ds, (Spatial.Segment3D)geometry3Ds[i], maxDistance);
                }
                else if (geometry3Ds[i] is Spatial.Polygon3D)
                {
                    geometry3Ds[i] = Spatial.Query.Snap((Spatial.Polygon3D)geometry3Ds[i], point3Ds, maxDistance);
                }
            }

            index = Params.IndexOfOutputParam("Geometry");
            if (index != -1)
            {
                dataAccess.SetData(index, geometry3Ds);
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, true);
            }
        }
    }
}
