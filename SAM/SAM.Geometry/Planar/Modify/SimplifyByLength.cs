using System;
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

            if (polyline2D.IsClosed())
            {
                Polygon2D polygon2D = SimplifyByLength(new Polygon2D(polyline2D.Points), maxLength, tolerance);
                if (polygon2D == null)
                    return null;

                return polygon2D.GetPolyline();
            }

            List<Segment2D> segment2Ds = polyline2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            //Collecting Intersections
            Dictionary<int, HashSet<Point2D>> dictionary = new Dictionary<int, HashSet<Point2D>>();
            for(int i =0; i < segment2Ds.Count; i++)
            {
                Segment2D segment2D = segment2Ds[i];

                HashSet<Point2D> point2Ds_Temp;
                if (!dictionary.TryGetValue(i, out point2Ds_Temp))
                {
                    point2Ds_Temp = new HashSet<Point2D>();
                    point2Ds_Temp.Add(segment2D.GetStart());
                    //point2Ds_Temp.Add(segment2D.GetEnd());
                    dictionary[i] = point2Ds_Temp;
                }

                Tuple<Point2D, Segment2D, Vector2D> aTuple;

                Point2D point2D_1 = segment2D.GetStart();
                Vector2D vector2D_1 = segment2D.Direction.GetNegated();

                aTuple = Query.TraceDataFirst(point2D_1, vector2D_1, segment2Ds);
                if(aTuple != null && aTuple.Item3.Length < maxLength)
                {
                    int index_Temp = Query.IndexOfClosest(segment2Ds, aTuple.Item1);

                    Segment2D segment2D_Temp = segment2Ds[index_Temp];

                    if(!dictionary.TryGetValue(index_Temp, out point2Ds_Temp))
                    {
                        point2Ds_Temp = new HashSet<Point2D>();
                        point2Ds_Temp.Add(segment2D.GetStart());
                        //point2Ds_Temp.Add(segment2D.GetEnd());
                        dictionary[i] = point2Ds_Temp;
                    }
                    point2Ds_Temp.Add(aTuple.Item1);
                }


                Point2D point2D_2 = segment2D.GetEnd();
                Vector2D vector2D_2 = segment2D.Direction;

                aTuple = Query.TraceDataFirst(point2D_2, vector2D_2, segment2Ds);
                if (aTuple != null && aTuple.Item3.Length < maxLength)
                {
                    int index_Temp = Query.IndexOfClosest(segment2Ds, aTuple.Item1);

                    Segment2D segment2D_Temp = segment2Ds[index_Temp];

                    if (!dictionary.TryGetValue(index_Temp, out point2Ds_Temp))
                    {
                        point2Ds_Temp = new HashSet<Point2D>();
                        point2Ds_Temp.Add(segment2D.GetStart());
                        //point2Ds_Temp.Add(segment2D.GetEnd());
                        dictionary[i] = point2Ds_Temp;
                    }
                    point2Ds_Temp.Add(aTuple.Item1);
                }


            }


            //Sorting
            List<Point2D> point2Ds = new List<Point2D>();
            foreach(KeyValuePair<int, HashSet<Point2D>> keyValuePair in dictionary)
            {
                List<Point2D> point2Ds_Temp = keyValuePair.Value.ToList();
                Point2D point2D = point2Ds_Temp.First();

                Modify.SortByDistance(point2Ds_Temp, point2D);

                point2Ds.AddRange(point2Ds_Temp);
            }

            point2Ds.Add(segment2Ds.Last().GetEnd());


            //Reorganizing
            List<Point2D> point2Ds_Result = new List<Point2D>();
            int index = 0;
            while(index < point2Ds.Count)
            {
                Point2D point2D = point2Ds[index];

                point2Ds_Result.Add(point2D);

                int index_Last = point2Ds.LastIndexOf(point2D);

                if(index != index_Last)
                {
                    if (index_Last == point2Ds.Count - 1)
                        break;

                    index = index_Last + 1;
                }
                
                index++;
            }

            if (point2Ds_Result == null || point2Ds_Result.Count == 0)
                return null;

            return new Polyline2D(point2Ds_Result);
        }
    }
}
