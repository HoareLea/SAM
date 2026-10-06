// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapPoints : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b9383b12-250f-4ff8-8b07-cc4aa6a33ff8");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// ` Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapPoints()
          : base("SAMAnalytical.SnapPoints", "SAMAnalytical.SnapPoints",
              "Generate Snap Points for SAM Analytical Panel",
              "SAM", "Analytical03")
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

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_Panel", NickName = "_Panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Point param_Point;
                param_Point = new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "_origin_", NickName = "_origin_", Description = "Origin Point", Access = GH_ParamAccess.item };
                param_Point.SetPersistentData(new Point3d(0, 0, 0));
                result.Add(new GH_SAMParam(param_Point, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_offset_", NickName = "_offset_", Description = "offset", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(1);
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Point() { Name = "_Points", NickName = "_Points", Description = "Points", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_Panel");
            Panel panel = null;
            if (index == -1 || !dataAccess.GetData(index, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_origin_");
            Point3d origin = new Point3d(0, 0, 0);
            if (index == -1 || !dataAccess.GetData(index, ref origin))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_offset_");
            double offset = 1;
            if (index == -1 || !dataAccess.GetData(index, ref offset))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IClosedPlanar3D> closedPlanar3Ds = panel.GetFace3D().GetEdge3Ds();
            if (closedPlanar3Ds == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Geometry");
                return;
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
            {
                ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                List<Point3D> point3Ds_Temp = segmentable3D.GetPoints();
                if (point3Ds_Temp == null)
                    continue;

                point3Ds.AddRange(point3Ds_Temp);
            }

            Geometry.Spatial.Modify.RemoveAlmostSimilar(point3Ds);

            Geometry.Spatial.Plane plane = panel.PlanarBoundary3D.Plane;
            Geometry.Planar.Point2D point2D_Project_Origin = plane.Convert(plane.Project(Geometry.Rhino.Convert.ToSAM(origin)));

            List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
            foreach (Point3D point3D in point3Ds)
            {
                Geometry.Planar.Point2D point2D_Project = plane.Convert(plane.Project(point3D));

                double x = System.Math.Round((point2D_Project_Origin.X - point2D_Project.X) / offset, 0, MidpointRounding.ToEven);
                double y = System.Math.Round((point2D_Project_Origin.Y - point2D_Project.Y) / offset, 0, MidpointRounding.ToEven);

                Geometry.Planar.Point2D point2D = new Geometry.Planar.Point2D(point2D_Project_Origin.X - (offset * x), point2D_Project_Origin.Y - (offset * y));

                point2Ds.Add(point2D);
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(0, offset)));
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(offset, offset)));
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(offset, 0)));
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(offset, -offset)));
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(0, -offset)));
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(-offset, -offset)));
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(-offset, 0)));
                point2Ds.Add(point2D.GetMoved(new Geometry.Planar.Vector2D(-offset, offset)));
            }

            index = Params.IndexOfOutputParam("_Points");
            if (index != -1)
            {
                dataAccess.SetDataList(index, point2Ds.ConvertAll(x => Geometry.Rhino.Convert.ToRhino(plane.Convert(x))));
            }
        }
    }
}
