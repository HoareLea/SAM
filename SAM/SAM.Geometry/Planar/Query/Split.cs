using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Split Segment2Ds
        /// </summary>
        /// <returns>List Segment2D</returns>
        /// <param name="segment2Ds">Segments2Ds</param>
        /// <param name="tolerance">tolerance</param>
        public static List<Segment2D> Split(this IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null)
                return null;

            List<Tuple<BoundingBox2D, Segment2D>> tuples = new List<Tuple<BoundingBox2D, Segment2D>>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                if(segment2D == null || segment2D.GetLength() < tolerance)
                {
                    continue;
                }

                tuples.Add(new Tuple<BoundingBox2D, Segment2D>(segment2D.GetBoundingBox(), segment2D));
            }

            int count = tuples.Count();

            Dictionary<int, List<Point2D>> dictionary = new Dictionary<int, List<Point2D>>();
            for (int i = 0; i < count - 1; i++)
            {
                BoundingBox2D boundingBox2D_1 = tuples[i].Item1;
                Segment2D segment2D_1 = tuples[i].Item2;
                
                for (int j = i + 1; j < count; j++)
                {
                    BoundingBox2D boundingBox2D_2 = tuples[j].Item1;
                    if (!boundingBox2D_1.InRange(boundingBox2D_2, tolerance))
                        continue;

                    Segment2D segment2D_2 = tuples[j].Item2;

                    if (segment2D_1.AlmostSimilar(segment2D_2, tolerance))
                        continue;

                    Point2D point2D_Closest1;
                    Point2D point2D_Closest2;

                    List<Point2D> point2Ds_Intersection = new List<Point2D>();

                    if (segment2D_1.On(segment2D_2[0], tolerance))
                        point2Ds_Intersection.Add(segment2D_2[0]);

                    if (segment2D_2.On(segment2D_1[0], tolerance))
                        point2Ds_Intersection.Add(segment2D_1[0]);

                    if (segment2D_1.On(segment2D_2[1], tolerance))
                        point2Ds_Intersection.Add(segment2D_2[1]);

                    if (segment2D_2.On(segment2D_1[1], tolerance))
                        point2Ds_Intersection.Add(segment2D_1[1]);

                    if (point2Ds_Intersection.Count == 0)
                    {
                        Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closest1, out point2D_Closest2, tolerance);
                        if (point2D_Intersection == null || point2D_Intersection.IsNaN())
                            continue;

                        if (point2D_Closest1 != null && point2D_Closest2 != null)
                            if (point2D_Closest1.Distance(point2D_Closest2) > tolerance)
                                continue;

                        point2Ds_Intersection.Add(point2D_Intersection);
                    }

                    foreach(Point2D point2D_Intersection in point2Ds_Intersection)
                    {
                        List<Point2D> points;

                        if (point2D_Intersection.Distance(segment2D_1.Start) > tolerance && point2D_Intersection.Distance(segment2D_1.End) > tolerance)
                        {
                            if (!dictionary.TryGetValue(i, out points))
                            {
                                points = new List<Point2D>();
                                dictionary[i] = points;
                            }

                            Modify.Add(points, point2D_Intersection, tolerance);
                        }

                        if (point2D_Intersection.Distance(segment2D_2.Start) > tolerance && point2D_Intersection.Distance(segment2D_2.End) > tolerance)
                        {
                            if (!dictionary.TryGetValue(j, out points))
                            {
                                points = new List<Point2D>();
                                dictionary[j] = points;
                            }

                            Modify.Add(points, point2D_Intersection, tolerance);
                        }
                    }
                }
            }

            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < count; i++)
            {
                Segment2D segment2D_Temp = tuples[i].Item2;
                if (result.Find(x => x.AlmostSimilar(segment2D_Temp, tolerance)) != null)
                    continue;

                List<Point2D> points;
                if (!dictionary.TryGetValue(i, out points))
                {
                    result.Add(segment2D_Temp);
                    continue;
                }

                Modify.Add(points, segment2D_Temp[0], tolerance);
                Modify.Add(points, segment2D_Temp[1], tolerance);

                Modify.SortByDistance(points, segment2D_Temp[0]);

                for (int j = 0; j < points.Count - 1; j++)
                {
                    Point2D point2D_1 = points[j];
                    Point2D point2D_2 = points[j + 1];

                    Segment2D segment2D = result.Find(x => (x[0].AlmostEquals(point2D_1, tolerance) && x[1].AlmostEquals(point2D_2, tolerance)) || (x[1].AlmostEquals(point2D_1, tolerance) && x[0].AlmostEquals(point2D_2, tolerance)));
                    if (segment2D != null)
                        continue;

                    result.Add(new Segment2D(point2D_1, point2D_2));
                }
            }

            return result;
        }

        public static List<Segment2D> Split(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp == null)
                    continue;

                segment2Ds.AddRange(segment2Ds_Temp);
            }

            return Split(segment2Ds, tolerance);
        }

        public static List<Segment2D> Split(this ISegmentable2D segmentable2D, IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null || segment2Ds == null)
                return null;

            if (segment2Ds.Count() == 0)
                return new List<Segment2D>();

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                List<Point2D> point2Ds = segmentable2D.Intersections(segment2D, tolerance);
                if (point2Ds == null || point2Ds.Count == 0)
                {
                    result.Add(new Segment2D(segment2D));
                    continue;
                }

                if (point2Ds.Find(x => x.AlmostEquals(segment2D[0])) == null)
                    point2Ds.Add(segment2D[0]);

                if (point2Ds.Find(x => x.AlmostEquals(segment2D[1])) == null)
                    point2Ds.Add(segment2D[1]);

                Modify.SortByDistance(point2Ds, segment2D[0]);

                List<Segment2D> segment2Ds_Temp = Create.Segment2Ds(point2Ds, false);
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count != 0)
                    result.AddRange(segment2Ds_Temp);
            }

            return result;
        }
        
        /// <summary>
        /// Method splits face2Ds by intersection. Does not fully work if any of face2Ds are similar or overlaping
        /// </summary>
        /// <param name="face2Ds">Face2Ds</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Face2Ds</returns>
        public static List<Face2D> Split(this IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2Ds == null)
                return null;

            if (face2Ds.Count() <= 1)
                return new List<Face2D>(face2Ds);

            List<Face2D> face2Ds_Temp = new List<Face2D>(face2Ds);
            face2Ds_Temp.RemoveSimilar(tolerance);

            MultiPolygon multiPolygon = face2Ds_Temp.ToNTS(tolerance);
            if (multiPolygon == null || multiPolygon.IsEmpty)
                return null;

            List<Polygon> polygons = multiPolygon.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            return polygons.ConvertAll(x => x.ToSAM(tolerance));
        }

        public static List<Face2D> Split(this Face2D face2D, IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance_Snap = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(face2D == null || segmentable2Ds == null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = face2D.GetBoundingBox();
            if(boundingBox2D == null)
            {
                return null;
            }

            List<ISegmentable2D> segmentable2Ds_All = new List<ISegmentable2D>();
            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                BoundingBox2D boundingBox2D_Segmentable2D = segmentable2D?.GetBoundingBox();
                if(boundingBox2D_Segmentable2D == null)
                {
                    continue;
                }

                if(!boundingBox2D.InRange(boundingBox2D_Segmentable2D, tolerance))
                {
                    continue;
                }

                segmentable2Ds_All.Add(segmentable2D);
            }

            if(segmentable2Ds_All == null || segmentable2Ds_All.Count == 0)
            {
                return null;
            }

            IClosed2D externalEdge = face2D.ExternalEdge2D;

            ISegmentable2D segmentable2D_ExternalEdge = externalEdge as ISegmentable2D;
            if(segmentable2D_ExternalEdge == null)
            {
                return null;
            }

            segmentable2Ds_All.Add(segmentable2D_ExternalEdge);

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if(internalEdges != null && internalEdges.Count != 0)
            {
                foreach(IClosed2D internalEdge in internalEdges)
                {
                    ISegmentable2D segmentable2D_InternalEdge = internalEdge as ISegmentable2D;
                    if(segmentable2D_InternalEdge == null)
                    {
                        continue;
                    }

                    segmentable2Ds_All.Add(segmentable2D_InternalEdge);
                }
            }

            List<Segment2D> segment2Ds = segmentable2Ds_All.Split(tolerance);
            segment2Ds = segment2Ds.Snap(true, tolerance_Snap);

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds, tolerance);
            if(polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            List<Tuple<Polygon2D, Point2D>> tuples = polygon2Ds.ConvertAll(x => new Tuple<Polygon2D, Point2D>(x, x.InternalPoint2D()));

            tuples = tuples.FindAll(x => externalEdge.Inside(x.Item2, tolerance));

            List<Face2D> result = new List<Face2D>();
            tuples.Sort((x, y) => y.Item1.GetArea().CompareTo(x.Item1.GetArea()));
            while(tuples.Count > 0)
            {
                Polygon2D polygon2D_External = tuples[0].Item1;
                tuples.RemoveAt(0);

                List<Polygon2D> polygon2Ds_Internal = tuples.FindAll(x => polygon2D_External.Inside(x.Item2, tolerance)).ConvertAll(x => x.Item1);
                if(polygon2Ds_Internal.Count != 0)
                {
                    tuples.RemoveAll(x => polygon2Ds_Internal.Contains(x.Item1));
                }

                Face2D face2D_Split = Create.Face2D(polygon2D_External, polygon2Ds_Internal);
                if (face2D_Split != null)
                {
                    Point2D point2D_Internal = face2D_Split?.GetInternalPoint2D(tolerance);
                    if (face2D.Inside(point2D_Internal, tolerance))
                    {
                        result.Add(face2D_Split);
                    }
                }
            }

            return result;

            //List<Tuple<Polygon2D, Point2D>> tuples_Internal = null;
            //if (internalEdges != null && internalEdges.Count != 0)
            //{
            //    tuples_Internal = tuples.FindAll(x => internalEdges.Find(y => y.Inside(x.Item2, tolerance)) != null);
            //}

            //tuples_Internal?.ForEach(x => tuples.Remove(x));

            //if(tuples.Count == 0)
            //{
            //    return null;
            //}

            //List<Face2D> result = new List<Face2D>();
            //foreach(Tuple<Polygon2D, Point2D> tuple in tuples)
            //{
            //    Face2D face2D_Split = Create.Face2D(tuple.Item1, tuples_Internal?.ConvertAll(x => x.Item1));
            //    if(face2D_Split != null)
            //    {
            //        result.Add(face2D_Split);
            //    }
            //}
            //return result;
        }

        public static List<Polyline2D> Split(this Polyline2D polyline2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D == null || point2D == null)
                return null;

            Point2D point2D_Temp = polyline2D.InsertClosest(point2D, tolerance);
            if (point2D_Temp == null)
                return null;

            int index = polyline2D.IndexOfClosestPoint2D(point2D_Temp);

            List<Point2D> point2Ds = polyline2D.Points;

            if (index == 0 || index == point2Ds.Count - 1)
                return new List<Polyline2D>() { new Polyline2D(point2Ds) };

            List<Polyline2D> result = new List<Polyline2D>();
            result.Add(new Polyline2D(point2Ds.GetRange(0, index + 1)));
            result.Add(new Polyline2D(point2Ds.GetRange(index, point2Ds.Count - index)));

            return result;
        }

        /// <summary>
        /// Generates additional points between two points where point2D_1 is first point on list and point2D_2 is last point.
        /// </summary>
        /// <param name="point2D_1">First point</param>
        /// <param name="point2D_2">Last point</param>
        /// <param name="count">Number of additional points</param>
        /// <returns></returns>
        public static IEnumerable<Point2D> Split(this Point2D point2D_1, Point2D point2D_2, int count)
        {
            if (point2D_1 == null || point2D_2 == null)
                return null;

            if (count <= 0)
                return null;

            if (count == 1)
                return new Point2D[] { point2D_1, point2D_2 };

            Vector2D vector2D = new Vector2D(point2D_1, point2D_2);
            double length_Split = vector2D.Length / count;
            vector2D = vector2D.Unit * length_Split;

            Point2D[] result = new Point2D[count + 1];

            result[0] = new Point2D(point2D_1);
            for (int i = 0; i < count; i++)
                result[i + 1] = result[i].GetMoved(vector2D);

            result[count] = new Point2D(point2D_2);

            return result;
        }

        /// <summary>
        /// Split one polygon2D into the count number of polygon2Ds. Split based on smallest Rectangle2D described on given polygon2D and spliting longest edge.
        /// </summary>
        /// <param name="polygon2D">Polygon2D to be splitted</param>
        /// <param name="count">Number of output Polygon2Ds</param>
        /// <param name="alignment">Split Alignment</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Polygon2Ds</returns>
        public static List<Polygon2D> Split(this Polygon2D polygon2D, int count, Alignment alignment, double tolerance = Core.Tolerance.Distance)
        {
            Rectangle2D rectangle2D = Create.Rectangle2D(polygon2D?.Points);
            if (rectangle2D == null)
                return null;

            Vector2D direction = Direction(alignment);
            if(direction == null)
                return null;

            double angle_Height = System.Math.Min(rectangle2D.HeightDirection.Angle(direction), rectangle2D.HeightDirection.Angle(direction.GetNegated()));
            double angle_Width = System.Math.Min(rectangle2D.WidthDirection.Angle(direction), rectangle2D.WidthDirection.Angle(direction.GetNegated()));

            Segment2D segment2D = null;
            Vector2D vector2D = null;

            if (angle_Height < angle_Width)
            {
                vector2D = rectangle2D.WidthDirection;
                Vector2D vector_Width = rectangle2D.WidthDirection * (rectangle2D.Width / 2);
                Point2D point2D_Start = rectangle2D.Origin.GetMoved(vector_Width);
                Point2D point2D_End = point2D_Start.GetMoved(rectangle2D.HeightDirection * rectangle2D.Height);

                segment2D = new Segment2D(point2D_Start, point2D_End);
            }
            else
            {
                vector2D = rectangle2D.HeightDirection;
                Vector2D vector_Height = rectangle2D.HeightDirection * (rectangle2D.Height / 2);
                Point2D point2D_Start = rectangle2D.Origin.GetMoved(vector_Height);
                Point2D point2D_End = point2D_Start.GetMoved(rectangle2D.WidthDirection * rectangle2D.Width);

                segment2D = new Segment2D(point2D_Start, point2D_End);
            }

            vector2D = vector2D * (rectangle2D.GetDiagonals()[0].GetLength() / 2);
            Vector2D vector2D_Negate = vector2D.GetNegated();

            IEnumerable<Point2D> point2Ds = Split(segment2D.Start, segment2D.End, count);

            if (point2Ds == null)
                return null;

            int aCount = point2Ds.Count();

            if (aCount <= 2)
                return new List<Polygon2D>() { new Polygon2D(polygon2D) };

            List<Polygon2D> result = new List<Polygon2D>();

            List<Segment2D> segment2Ds = new List<Segment2D>();
            for (int i = 1; i < aCount - 1; i++)
            {
                Point2D point2D = point2Ds.ElementAt(i);
                segment2Ds.Add(new Segment2D(point2D.GetMoved(vector2D), point2D.GetMoved(vector2D_Negate)));
            }

            segment2Ds.AddRange(polygon2D.GetSegments());

            if (segment2Ds == null || segment2Ds.Count < 4)
                return new List<Polygon2D>() { new Polygon2D(polygon2D) };

            segment2Ds = Split(segment2Ds, tolerance);
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Segment2D> segment2Ds_Inside = new List<Segment2D>();
            foreach (Segment2D segment2D_Temp in segment2Ds)
            {
                Point2D point2D = segment2D_Temp.Mid();

                if (!polygon2D.Inside(point2D, tolerance) && !polygon2D.On(point2D, tolerance))
                    continue;

                segment2Ds_Inside.Add(segment2D_Temp);
            }

            return Create.Polygon2Ds(segment2Ds_Inside, tolerance);
        }
        
        public static List<Tuple<Face2D, T>> Split<T>(this Face2D face2D, IEnumerable<Tuple<Face2D, T>> tuples, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null || tuples == null || tuples.Count() == 0)
            {
                return null;
            }

            List<Face2D> face2Ds_Temp = new List<Face2D>() { face2D };

            List<Tuple<Face2D, T>> result = new List<Tuple<Face2D, T>>();
            foreach (Tuple<Face2D, T> tuple in tuples)
            {
                List<Face2D> face2Ds_Intersection = new List<Face2D>();
                foreach(Face2D face2D_Temp in face2Ds_Temp)
                {
                    List<Face2D> face2Ds_Intersection_Temp = face2D_Temp.Intersection(tuple.Item1);
                    face2Ds_Intersection_Temp?.RemoveAll(x => x == null || x.GetArea() <= tolerance);
                    if (face2Ds_Intersection_Temp == null || face2Ds_Intersection_Temp.Count == 0)
                    {
                        continue;
                    }

                    face2Ds_Intersection.AddRange(face2Ds_Intersection_Temp);
                }

                result.AddRange(face2Ds_Intersection.ConvertAll(x => new Tuple<Face2D, T>(x, tuple.Item2)));

                List<Face2D> face2Ds_Difference = new List<Face2D>();
                foreach (Face2D face2D_Temp in face2Ds_Temp)
                {
                    List<Face2D> face2Ds_Difference_Temp = face2D_Temp.Difference(face2Ds_Intersection);
                    face2Ds_Difference_Temp?.RemoveAll(x => x == null || x.GetArea() <= tolerance);
                    if (face2Ds_Difference_Temp == null || face2Ds_Difference_Temp.Count == 0)
                    {
                        continue;
                    }

                    face2Ds_Difference.AddRange(face2Ds_Difference_Temp);
                }

                face2Ds_Temp = face2Ds_Difference;
                if (face2Ds_Temp == null || face2Ds_Temp.Count == 0)
                    break;
            }

            if (face2Ds_Temp != null && face2Ds_Temp.Count != 0)
            {
                int index = -1;
                while(index < face2Ds_Temp.Count)
                {
                    index++;

                    for (int i = 0; i < result.Count; i++)
                    {
                        List<Face2D> face2Ds_Union = result[i].Item1.Union(face2Ds_Temp[index]);
                        if (face2Ds_Union == null || face2Ds_Union.Count != 1)
                        {
                            continue;
                        }

                        face2Ds_Temp.RemoveAt(index);
                        result[i] = new Tuple<Face2D, T>(face2Ds_Union[0], result[i].Item2);
                        index = -1;
                        break;
                    }
                }

                if (face2Ds_Temp != null && face2Ds_Temp.Count != 0)
                {
                    List<Tuple<Face2D, T>> tuples_Temp_Sorted = new List<Tuple<Face2D, T>>(result);
                    tuples_Temp_Sorted.Sort((x, y) => y.Item1.GetArea().CompareTo(x.Item1.GetArea()));

                    face2Ds_Temp = face2Ds_Temp.Union(tolerance);

                    foreach (Face2D face2D_Temp in face2Ds_Temp)
                    {
                        result.Add(new Tuple<Face2D, T>(face2D_Temp, tuples_Temp_Sorted[0].Item2));
                    }
                }

            }

            return result;

        }
    }
}