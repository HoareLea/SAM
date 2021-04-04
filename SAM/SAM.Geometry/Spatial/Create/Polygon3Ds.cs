using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Polygon3D> Polygon3Ds(this IEnumerable<IClosedPlanar3D> closedPlanar3Ds, Plane plane, bool checkIntersection = true, double tolerance = Core.Tolerance.Distance)
        {
            if (closedPlanar3Ds == null || plane == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            for (int i = 0; i < closedPlanar3Ds.Count(); i++)
            {
                IClosedPlanar3D closedPlanar3D = closedPlanar3Ds.ElementAt(i);
                if (closedPlanar3D == null)
                    continue;

                if (closedPlanar3D is Face3D)
                {
                    closedPlanar3D = ((Face3D)closedPlanar3D).GetExternalEdge3D();
                    if (closedPlanar3D == null)
                        continue;
                }

                ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                Plane plane_Temp = closedPlanar3D.GetPlane();
                if (plane_Temp == null)
                    continue;

                PlanarIntersectionResult planarIntersectionResult = null;
                if(checkIntersection)
                {
                    planarIntersectionResult = PlanarIntersectionResult(plane, closedPlanar3D);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                        continue;
                }

                planarIntersectionResult = PlanarIntersectionResult(plane, plane_Temp);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                Line3D line3D = planarIntersectionResult.GetGeometry3D<Line3D>();
                if (line3D == null)
                    continue;

                List<Point3D> point3Ds = segmentable3D.GetPoints();
                if (point3Ds == null || point3Ds.Count == 0)
                    continue;

                point3Ds = point3Ds.ConvertAll(x => line3D.Project(x));

                Point3D point3D_1 = null;
                Point3D point3D_2 = null;
                Query.ExtremePoints(point3Ds, out point3D_1, out point3D_2);
                if (point3D_1 == null || point3D_2 == null || point3D_1.AlmostEquals(point3D_2, tolerance))
                    continue;

                Segment3D segment3D = new Segment3D(point3D_1, point3D_2);

                segment2Ds.Add(plane.Convert(segment3D));
            }

            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            segment2Ds.Snap(true, tolerance);

            segment2Ds = segment2Ds.Split(tolerance);

            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            polygon2Ds = polygon2Ds.Union(tolerance);

            List<Polygon3D> result = new List<Polygon3D>();

            foreach (Polygon2D polygon2D in polygon2Ds)
            {
                Polygon2D polygon2D_Temp = Planar.Query.Snap(polygon2D, segment2Ds, tolerance);
                if (polygon2D_Temp != null)
                    polygon2D_Temp = polygon2D;

                result.Add(plane.Convert(polygon2D_Temp));
            }

            return result;
        }
    }
}