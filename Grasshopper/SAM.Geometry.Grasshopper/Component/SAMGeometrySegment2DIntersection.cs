// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public class SAMGeometrySegment2DIntersection : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2328b29d-21c2-4ad6-940f-482a8cdc6b68");

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
        public SAMGeometrySegment2DIntersection()
          : base("SAMGeometry.Segment2DIntersection", "GHgeo",
              "Segment2D Intersection",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_1stSegment2D", NickName = "_1stSegment2D", Description = "SAM Geometry segment2D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_2ndSegment2D", NickName = "_2ndSegment2D", Description = "SAM Geometry segment2D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Point2D", NickName = "Pt2D", Description = "Intersection between segment2Ds SAM Point2D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "1stClosestPoint2D", NickName = "1stCPt2D", Description = "First closest SAM Point2D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "2ndClosestPoint2D", NickName = "2ndCPt2D", Description = "Second closest SAM Point2D", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper = null;

            index = Params.IndexOfInputParam("_1stSegment2D");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Segment2D segment2D_1 = objectWrapper.Value as Segment2D;
            if (segment2D_1 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid segment");
                return;
            }

            index = Params.IndexOfInputParam("_2ndSegment2D");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Segment2D segment2D_2 = objectWrapper.Value as Segment2D;
            if (segment2D_2 == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid segment");
                return;
            }

            Point2D point2D_Closest_1 = null;
            Point2D point2D_Closest_2 = null;

            Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closest_1, out point2D_Closest_2);

            index = Params.IndexOfOutputParam("Point2D");
            if (index != -1)
            {
                dataAccess.SetData(index, point2D_Intersection);
            }

            index = Params.IndexOfOutputParam("1stClosestPoint2D");
            if (index != -1)
            {
                dataAccess.SetData(index, point2D_Closest_1);
            }

            index = Params.IndexOfOutputParam("2ndClosestPoint2D");
            if (index != -1)
            {
                dataAccess.SetData(index, point2D_Closest_2);
            }

            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot split segments");
        }
    }
}
