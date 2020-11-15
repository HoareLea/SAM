using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Triangle2D> Triangulate(this Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment2D> segment2Ds = polygon2D?.GetSegments();
            if (segment2Ds == null)
                return null;

            segment2Ds.RemoveAll(x => x == null);
            if (segment2Ds.Count < 3)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            bool updated = true;
            while (updated && segment2Ds.Count > 0)
            {
                updated = false;
                for(int i=0; i < segment2Ds.Count - 1; i++)
                {
                    Segment2D segment2D_1 = segment2Ds[i];
                    Segment2D segment2D_2 = segment2Ds[i + 1];

                    Point2D point2D_1 = segment2D_1[0];
                    Point2D point2D_2 = segment2D_2[1];

                    Segment2D segment2D_3 = new Segment2D(point2D_1, point2D_2);

                    List<Point2D> point2Ds = Intersections(segment2D_3, segment2Ds, tolerance);
                    if (point2Ds == null || point2Ds.Count == 0)
                        continue;

                    point2Ds.RemoveAll(x => x.AlmostEquals(point2D_1, tolerance) || x.AlmostEquals(point2D_2, tolerance));

                    if (point2Ds.Count > 0)
                        continue;

                    Triangle2D triangle2D = new Triangle2D(point2D_1, segment2D_2[0], point2D_2);
                    result.Add(triangle2D);
                    
                    segment2Ds.Remove(segment2D_1);
                    segment2Ds.Remove(segment2D_2);
                    updated = true;
                }
            }

            return result;
        }

        public static List<Triangle2D> Triangulate(this Polyline2D polyline2D, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D == null)
                return null;

            if (!polyline2D.IsClosed(tolerance))
                return null;

            return new Polygon2D(polyline2D.Points).Triangulate(tolerance);
        }

        public static List<Triangle2D> Triangulate(this Rectangle2D rectangle2D)
        {
            List<Segment2D> segment2Ds = rectangle2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count != 4)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            result.Add(new Triangle2D(segment2Ds[0][0], segment2Ds[0][1], segment2Ds[1][1]));
            result.Add(new Triangle2D(segment2Ds[2][0], segment2Ds[2][1], segment2Ds[3][1]));
            return result;
        }

        public static List<Triangle2D> Triangulate(this BoundingBox2D boundingBox2D)
        {
            List<Segment2D> segment2Ds = boundingBox2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count != 4)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            result.Add(new Triangle2D(segment2Ds[0][0], segment2Ds[0][1], segment2Ds[1][1]));
            result.Add(new Triangle2D(segment2Ds[2][0], segment2Ds[2][1], segment2Ds[3][1]));
            return result;
        }

        public static List<Triangle2D> Triangulate(this Triangle2D triangle2D)
        {
            if (triangle2D == null)
                return null;
            
            return new List<Triangle2D>() { new Triangle2D(triangle2D) };
        }
    }
}