using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Panel Extend(this Panel panel, IEnumerable<Panel> panels, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null || panels.Count() == 0)
                return null;

            Plane plane = panel.Plane;
            if (plane == null)
                return null;

            List<Segment2D> segment2Ds_Panels = new List<Segment2D>();
            foreach (Panel panel_Temp in panels)
            {
                Face3D face3D_Temp = panel_Temp?.GetFace3D();
                if (face3D_Temp == null)
                    continue;

                Plane plane_Temp = face3D_Temp.GetPlane();
                if (plane_Temp == null)
                    continue;

                PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(plane, plane_Temp);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                Line3D line3D = planarIntersectionResult.GetGeometry3D<Line3D>();
                if (line3D == null)
                    continue;

                ISegmentable3D segmentable3D_Temp = face3D_Temp.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D_Temp == null)
                    continue;

                List<Point3D> point3Ds = segmentable3D_Temp.GetPoints();
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                point3Ds = point3Ds.ConvertAll(x => line3D.Project(x));

                Point3D point3D_1 = null;
                Point3D point3D_2 = null;
                Geometry.Spatial.Query.ExtremePoints(point3Ds, out point3D_1, out point3D_2);
                if (point3D_1 == null || point3D_2 == null || point3D_1.AlmostEquals(point3D_2, tolerance))
                    continue;

                Segment3D segment3D = new Segment3D(point3D_1, point3D_2);

                segment2Ds_Panels.Add(plane.Convert(segment3D));
            }

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
                return null;

            List<Segment3D> segment3Ds = segmentable3D.GetSegments();
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach(Segment3D segment3D in segment3Ds)
            {
                Segment2D segment2D = plane.Convert(segment3D);
                if (segment2D == null)
                    continue;

                Vector2D vector2D_1 = Geometry.Planar.Query.TraceFirst(segment2D[0], segment2D.Direction.GetNegated(), segment2Ds_Panels);
                Vector2D vector2D_2 = Geometry.Planar.Query.TraceFirst(segment2D[1], segment2D.Direction, segment2Ds_Panels);

                if (vector2D_1 != null)
                    segment2D = new Segment2D(segment2D[0].GetMoved(vector2D_1), segment2D[1]);

                if(vector2D_2 != null)
                    segment2D = new Segment2D(segment2D[0], segment2D[1].GetMoved(vector2D_2));

                segment2Ds.Add(segment2D);
            }

            segment2Ds.AddRange(segment2Ds_Panels);

            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, tolerance);
            polygon2Ds = Geometry.Planar.Query.Union(polygon2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            Polygon2D polygon2D = polygon2Ds[0];
            if (polygon2D.GetArea() < panel.GetArea())
                return null;

            face3D = Face3D.Create(plane, polygon2D, face3D.InternalEdge2Ds, true);
            if (face3D == null)
                return null;

            return new Panel(panel.Guid, panel, face3D, null, true, 0, double.MaxValue);
        }
    }
}