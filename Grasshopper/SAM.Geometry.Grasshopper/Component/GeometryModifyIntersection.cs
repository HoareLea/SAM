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
    public class GeometryModifyIntersection : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c8abbba4-bfc7-4d7f-a6cb-0afd6f422ed5");

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
        public GeometryModifyIntersection()
          : base("Geometry.ModifyIntersection", "Geometry.tuples_LinearRing",
              "ModifyIntersection from Polygons",
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

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_polygon1", NickName = "_polygon1", Description = "Polygon 1", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_polygon2", NickName = "_polygon2", Description = "Polygon 2", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(Core.Tolerance.Distance);
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
                result.Add(new GH_SAMParam(new GooSAMGeometryParam() { Name = "Polygon", NickName = "Polygon", Description = "Polygon", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_polygon1");
            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Planar.Polygon2D> polygon2Ds_1 = new List<Planar.Polygon2D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Planar.Polygon2D polygon2D = gHObjectWrapper.Value as Planar.Polygon2D;
                if (polygon2D != null)
                {
                    polygon2Ds_1.Add(polygon2D);
                    continue;
                }
            }

            index = Params.IndexOfInputParam("_polygon2");
            objectWrapperList = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Planar.Polygon2D> polygon2Ds_2 = new List<Planar.Polygon2D>();
            foreach (GH_ObjectWrapper gHObjectWrapper in objectWrapperList)
            {
                Planar.Polygon2D polygon2D = gHObjectWrapper.Value as Planar.Polygon2D;
                if (polygon2D != null)
                {
                    polygon2Ds_2.Add(polygon2D);
                    continue;
                }
            }

            List<Planar.Polygon2D> polygon2Ds_result = Planar.Query.Intersection(polygon2Ds_1[0], polygon2Ds_2[0], tolerance);

            index = Params.IndexOfOutputParam("Polygon");
            if (index != -1)
            {
                dataAccess.SetDataList(index, polygon2Ds_result.ConvertAll(x => new GooSAMGeometry(x)));
            }
        }
    }
}
