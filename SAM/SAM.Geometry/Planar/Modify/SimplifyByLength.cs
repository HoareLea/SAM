using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static Polygon2D SimplifyByLength(this Polygon2D polygon2D, double maxLength, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon2D == null || polygon2D.Count < 3 || double.IsNaN(maxLength) )
                return null;

            Polygon2D polygon2D_Temp = new Polygon2D(polygon2D);

            List<Segment2D> segment2Ds_Temp = Query.SelfIntersectionSegment2Ds(polygon2D_Temp, maxLength, tolerance);

            List<Polygon2D> polygon2Ds = new PointGraph2D(segment2Ds_Temp, true, tolerance).GetPolygon2Ds();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return polygon2D;

            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            polygon2D_Temp = polygon2Ds.Last();

            segment2Ds_Temp = Query.SelfIntersectionSegment2Ds(polygon2D_Temp, maxLength, tolerance);
            polygon2Ds = new PointGraph2D(segment2Ds_Temp, true, tolerance).GetPolygon2Ds_External();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return polygon2D;

            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            polygon2D_Temp = polygon2Ds.Last();

            List<Point2D> point2Ds = polygon2D_Temp.GetPoints();
            segment2Ds_Temp = new List<Segment2D>();
            for (int i = 0; i < point2Ds.Count - 1; i++)
                for (int j = i; j < point2Ds.Count; j++)
                {
                    if (point2Ds[i].Distance(point2Ds[j]) <= maxLength)
                    {
                        Segment2D segment2D_Intersection = new Segment2D(point2Ds[i], point2Ds[j]);
                        if (segment2Ds_Temp.Find(x => x.AlmostSimilar(segment2D_Intersection, tolerance)) == null)
                            segment2Ds_Temp.Add(segment2D_Intersection);
                    }
                }

            segment2Ds_Temp.AddRange(polygon2D_Temp.GetSegments());
            polygon2Ds = new PointGraph2D(segment2Ds_Temp, true, tolerance).GetPolygon2Ds();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return polygon2D;

            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            polygon2D_Temp = polygon2Ds.Last();

            point2Ds = polygon2D_Temp.GetPoints();
            if (point2Ds == null)
                return polygon2D;

            return new Polygon2D(point2Ds);
        }
    
        public static Polyline2D SimplifyByLength(this Polyline2D polyline2D, double maxLength, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D == null || double.IsNaN(maxLength))
                return null;

            if(polyline2D.IsClosed())
            {
                Polygon2D polygon2D = SimplifyByLength(new Polygon2D(polyline2D.Points), maxLength, tolerance);
                if (polygon2D == null)
                    return null;

                return polygon2D.GetPolyline();
            }

            List<Segment2D> segment2Ds = polyline2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Segment2D> segment2s_Result = new List<Segment2D>();

            while(segment2Ds.Count > 0)
            {
                Segment2D segment2D = segment2Ds[0];

                Point2D point2D_1 = segment2D.GetStart();
                Vector2D vector2D_1 = segment2D.Direction.GetNegated();

                Vector2D vector2D_Intersection_1 = Query.TraceFirst(point2D_1, vector2D_1, (ISegmentable2D)polyline2D);
                if (vector2D_Intersection_1 != null && vector2D_Intersection_1.Length > maxLength)
                    vector2D_Intersection_1 = null;

                Point2D point2D_2 = segment2D.GetEnd();
                Vector2D vector2D_2 = segment2D.Direction;

                Vector2D vector2D_Intersection_2 = Query.TraceFirst(point2D_2, vector2D_2, (ISegmentable2D)polyline2D);
                if (vector2D_Intersection_2 != null && vector2D_Intersection_2.Length > maxLength)
                    vector2D_Intersection_2 = null;

                if (vector2D_Intersection_1 != null || vector2D_Intersection_2 != null)
                {
                    if(vector2D_Intersection_1 != null)
                    {

                    }

                    if (vector2D_Intersection_2 != null)
                    {

                    }
                }
                else
                {
                    segment2s_Result.Add(segment2D);

                }

                segment2Ds.RemoveAt(0);
            }

            for (int i = 1; i < segment2Ds.Count - 1; i++)
            {
                
                




                //point2D = segment2D.GetEnd();
                //vector2D = segment2D.Direction;

                //vector2D_Intersection = Query.TraceFirst(point2D, vector2D, (ISegmentable2D)polyline2D);
                //if (vector2D_Intersection != null && vector2D_Intersection.Length > 0)
                //{
                //    Segment2D segment2D_Intersection = new Segment2D(point2D.GetMoved(vector2D_Intersection), point2D);
                //    if (segment2D_Intersection.GetLength() > maxLength)
                //        continue;

                //    List<Point2D> point2Ds_Intersections = polygon2D.Intersections(segment2D_Intersection, tolerance);
                //    if (point2Ds_Intersections != null && point2Ds_Intersections.Count == 2)
                //        if (result.Find(x => x.AlmostSimilar(segment2D_Intersection, tolerance)) == null)
                //            result.Add(segment2D_Intersection);
                //}
            }

            throw new System.Exception();
        }
    }
}
