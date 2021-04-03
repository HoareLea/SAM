using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Face3D Extend(this Face3D face3D, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3D == null || plane == null)
                return null;

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
                return null;

            Plane plane_Face3D = face3D.GetPlane();

            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(plane, plane_Face3D, tolerance_Angle);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            Line3D line3D_Intersection = planarIntersectionResult.GetGeometry3D<Line3D>();
            if (line3D_Intersection == null)
                return null; ;

            Dictionary<Point2D, Segment2D> dictionary = new Dictionary<Point2D, Segment2D>();
            foreach (Point3D point3D in segmentable3D.GetPoints())
            {
                Point3D point3D_Temp = line3D_Intersection.Project(point3D);
                if (point3D_Temp == null)
                    continue;

                dictionary[plane_Face3D.Convert(point3D_Temp)] = plane_Face3D.Convert(new Segment3D(point3D_Temp, point3D));
            }

            if (dictionary == null || dictionary.Count < 2)
                return null;

            Planar.Query.ExtremePoints(dictionary.Keys, out Point2D point2D_1, out Point2D point2D_2);

            if (point2D_1.Distance(point2D_2) < tolerance_Distance)
                return null;

            ISegmentable2D segmentable2D = face3D.ExternalEdge2D as ISegmentable2D;
            List<Segment2D> segment2Ds = segmentable2D.GetSegments();
            segment2Ds.Add(new Segment2D(point2D_1, point2D_2));
            segment2Ds.Add(dictionary[point2D_1]);
            segment2Ds.Add(dictionary[point2D_2]);

            segment2Ds = Planar.Query.Split(segment2Ds, tolerance_Distance);

            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
            polygon2Ds = Planar.Query.Union(polygon2Ds, tolerance_Distance);

            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            if (polygon2Ds.Count > 1)
                polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            Polygon2D polygon2D = polygon2Ds[0];

            polygon2D.SetOrientation(segmentable2D.GetPoints().Orientation());

            return Face3D.Create(plane_Face3D, polygon2D, face3D.InternalEdge2Ds);
        }
    }
}