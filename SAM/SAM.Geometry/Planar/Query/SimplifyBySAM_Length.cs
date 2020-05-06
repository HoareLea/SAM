using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon2D SimplifyBySAM_Length(this Polygon2D polygon2D, double maxLength, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon2D == null || polygon2D.Count < 3 || double.IsNaN(maxLength))
                return null;

            Polygon2D polygon2D_Temp = new Polygon2D(polygon2D);

            List<Segment2D> segment2Ds_Temp = SelfIntersectionSegment2Ds(polygon2D_Temp, maxLength, tolerance);

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds_Temp, tolerance); //new PointGraph2D(segment2Ds_Temp, true, tolerance).GetPolygon2Ds();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return polygon2D;

            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            polygon2D_Temp = polygon2Ds.Last();

            segment2Ds_Temp = SelfIntersectionSegment2Ds(polygon2D_Temp, maxLength, tolerance);
            polygon2Ds = ExternalPolygon2Ds(segment2Ds_Temp, tolerance);//new PointGraph2D(segment2Ds_Temp, true, tolerance).GetPolygon2Ds_External();
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
            polygon2Ds = Create.Polygon2Ds(segment2Ds_Temp, tolerance); //new PointGraph2D(segment2Ds_Temp, true, tolerance).GetPolygon2Ds();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return polygon2D;

            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            polygon2D_Temp = polygon2Ds.Last();

            point2Ds = polygon2D_Temp.GetPoints();
            if (point2Ds == null)
                return polygon2D;

            return new Polygon2D(point2Ds);
        }

        public static Polyline2D SimplifyBySAM_Length(this Polyline2D polyline2D, double maxLength, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D == null || double.IsNaN(maxLength))
                return null;

            if (polyline2D.IsClosed())
            {
                Polygon2D polygon2D = SimplifyBySAM_Length(new Polygon2D(polyline2D.Points), maxLength, tolerance);
                if (polygon2D == null)
                    return null;

                return polygon2D.GetPolyline();
            }

            List<Segment2D> segment2Ds = polyline2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            segment2Ds = Split(segment2Ds, tolerance);

            //Collecting Intersections
            Dictionary<int, HashSet<Point2D>> dictionary = new Dictionary<int, HashSet<Point2D>>();
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                Segment2D segment2D = segment2Ds[i];

                HashSet<Point2D> point2Ds_Temp;
                if (!dictionary.TryGetValue(i, out point2Ds_Temp))
                {
                    point2Ds_Temp = new HashSet<Point2D>();
                    point2Ds_Temp.Add(segment2D.GetStart());
                    point2Ds_Temp.Add(segment2D.GetEnd());
                    dictionary[i] = point2Ds_Temp;
                }

                Tuple<Point2D, Segment2D, Vector2D> aTuple;

                Point2D point2D_1 = segment2D.GetStart();
                Vector2D vector2D_1 = segment2D.Direction.GetNegated();

                aTuple = TraceDataFirst(point2D_1, vector2D_1, segment2Ds);
                if (aTuple != null && aTuple.Item3.Length < maxLength)
                {
                    int index_Temp = IndexOfClosestSegment2D(segment2Ds, aTuple.Item1);

                    Segment2D segment2D_Temp = segment2Ds[index_Temp];

                    if (!dictionary.TryGetValue(index_Temp, out point2Ds_Temp))
                    {
                        point2Ds_Temp = new HashSet<Point2D>();
                        point2Ds_Temp.Add(segment2D_Temp.GetStart());
                        point2Ds_Temp.Add(segment2D_Temp.GetEnd());
                        dictionary[index_Temp] = point2Ds_Temp;
                    }
                    point2Ds_Temp.Add(aTuple.Item1);
                    dictionary[i].Add(aTuple.Item1);
                }

                Point2D point2D_2 = segment2D.GetEnd();
                Vector2D vector2D_2 = segment2D.Direction;

                aTuple = TraceDataFirst(point2D_2, vector2D_2, segment2Ds);
                if (aTuple != null && aTuple.Item3.Length < maxLength)
                {
                    int index_Temp = IndexOfClosestSegment2D(segment2Ds, aTuple.Item1);

                    Segment2D segment2D_Temp = segment2Ds[index_Temp];

                    if (!dictionary.TryGetValue(index_Temp, out point2Ds_Temp))
                    {
                        point2Ds_Temp = new HashSet<Point2D>();
                        point2Ds_Temp.Add(segment2D_Temp.GetStart());
                        point2Ds_Temp.Add(segment2D_Temp.GetEnd());
                        dictionary[index_Temp] = point2Ds_Temp;
                    }
                    point2Ds_Temp.Add(aTuple.Item1);
                    dictionary[i].Add(aTuple.Item1);
                }
            }

            //Sorting
            List<int> indexes = dictionary.Keys.ToList();
            indexes.Sort();

            Point2D point2D_Start = dictionary[indexes.First()].ToList().First();

            List<List<Point2D>> point2Ds = new List<List<Point2D>>();
            foreach (int index_Temp in indexes)
            {
                List<Point2D> point2Ds_Temp = dictionary[index_Temp].ToList();
                Modify.SortByDistance(point2Ds_Temp, point2D_Start);

                point2Ds.Add(point2Ds_Temp);
                point2D_Start = point2Ds_Temp.Last();
            }

            //Reorganizing
            int index = 0;

            List<Point2D> point2Ds_Result = new List<Point2D>();
            point2Ds_Result.Add(point2Ds.First().First());
            while (index < point2Ds.Count)
            {
                int index_Next = -1;
                Point2D point2D_Next = null;

                GetNext(point2Ds, index, out index_Next, out point2D_Next, tolerance);
                if (index_Next == -1 || index_Next == index)
                    break;

                point2Ds_Result.Add(point2D_Next);
                index = index_Next;
            }

            point2Ds_Result.Add(point2Ds.Last().Last());

            return new Polyline2D(point2Ds_Result);
        }

        private static void GetNext(List<List<Point2D>> point2Ds, int index, out int index_Next, out Point2D point2D_Next, double tolerance = Core.Tolerance.Distance)
        {
            index_Next = -1;
            point2D_Next = null;

            List<Point2D> point2Ds_Current = point2Ds[index];
            List<int> indexes = point2Ds_Current.ConvertAll(x => -1);

            for (int i = 0; i < point2Ds.Count; i++)
                for (int j = 0; j < point2Ds_Current.Count; j++)
                    if (point2Ds[i].Contains(point2Ds_Current[j], tolerance))
                        indexes[j] = i;

            for (int i = 0; i < point2Ds_Current.Count; i++)
            {
                if (indexes[i] > index_Next)
                {
                    index_Next = indexes[i];
                    point2D_Next = point2Ds_Current[i];
                }
            }
        }
    }
}