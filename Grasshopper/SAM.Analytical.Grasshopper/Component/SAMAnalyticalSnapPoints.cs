using Grasshopper.Kernel;
using Rhino.Geometry;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSnapPoints : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b9383b12-250f-4ff8-8b07-cc4aa6a33ff8");

        /// <summary>
        /// ` Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSnapPoints()
          : base("SAMAnalytical.SnapPoints", "SAMAnalytical.SnapPoints",
              "Generate Snap Points for SAM Analytical Panel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooPanelParam(), "_Panel", "_Panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddPointParameter("_origin_", "_origin_", "Origin Point", GH_ParamAccess.item, new Point3d(0, 0, 0));
            inputParamManager.AddNumberParameter("_offset_", "_offset_", "offset", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddPointParameter("_Points", "_Points", "Points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Point3d origin = new Point3d(0, 0, 0);
            if (!dataAccess.GetData(1, ref origin))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double offset = 1;
            if (!dataAccess.GetData(2, ref offset))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IClosedPlanar3D> closedPlanar3Ds = panel.GetFace3D().GetEdges();
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
            Geometry.Planar.Point2D point2D_Project_Origin = plane.Convert(plane.Project(Geometry.Grasshopper.Convert.ToSAM(origin)));

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

            dataAccess.SetDataList(0, point2Ds.ConvertAll(x => Geometry.Grasshopper.Convert.ToRhino(plane.Convert(x))));
        }
    }
}