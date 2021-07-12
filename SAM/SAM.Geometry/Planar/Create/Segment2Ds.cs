using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, Alignment alignment, double offset, Corner corner = Corner.TopLeft, bool includeEdge = true)
        {
            double offset_Horizontal = 0;
            double offset_Vertical = 0;
            switch (alignment)
            {
                case Alignment.Horizontal:
                    offset_Horizontal = offset;
                    break;

                case Alignment.Vertical:
                    offset_Vertical = offset;
                    break;

                default:
                    return null;
            }

            return Segment2Ds(boundingBox2D, offset_Horizontal, offset_Vertical, corner, includeEdge);
        }

        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, double? horizontalOffset, double? verticalOffset, Point2D point2D, bool includeEdge = true)
        {
            return Segment2Ds(boundingBox2D, horizontalOffset, verticalOffset, boundingBox2D.GetCorner(point2D), true);
        }

        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, Alignment alignment, double offset, Point2D point2D, bool includeEdge = true)
        {
            return Segment2Ds(boundingBox2D, alignment, offset, boundingBox2D.GetCorner(point2D), true);
        }

        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, double? horizontalOffset, double? verticalOffset, Corner corner = Corner.TopLeft, bool includeEdge = true)
        {
            if (boundingBox2D == null || corner == Corner.Undefined)
                return null;

            List<Segment2D> result = null;

            if (horizontalOffset != null && horizontalOffset.HasValue && horizontalOffset.Value != 0)
            {
                result = new List<Segment2D>();

                double x_Start = boundingBox2D.Min.X;
                double x_End = boundingBox2D.Max.X;

                double y_Start = double.NaN;
                double factor_direction = 1;
                switch (corner)
                {
                    case Corner.TopLeft:
                    case Corner.TopRight:
                        y_Start = boundingBox2D.Max.Y;
                        factor_direction = -1;
                        break;

                    case Corner.BottomLeft:
                    case Corner.BottomRight:
                        y_Start = boundingBox2D.Min.Y;
                        factor_direction = 1;
                        break;
                }

                double offset = horizontalOffset.Value;
                int count = System.Convert.ToInt32(boundingBox2D.Height / offset);
                if (!includeEdge)
                    count--;
                else
                    count++;

                offset = offset * factor_direction;
                for (int i = 0; i < count; i++)
                {
                    int index = i;
                    if (!includeEdge)
                        index++;

                    double y = y_Start + (offset * index);
                    result.Add(new Segment2D(new Point2D(x_Start, y), new Point2D(x_End, y)));
                }
            }

            if (verticalOffset != null && verticalOffset.HasValue && verticalOffset.Value != 0)
            {
                if (result == null)
                    result = new List<Segment2D>();

                double y_Start = boundingBox2D.Min.Y;
                double y_End = boundingBox2D.Max.Y;

                double x_Start = double.NaN;
                double factor_direction = 1;
                switch (corner)
                {
                    case Corner.TopLeft:
                    case Corner.BottomLeft:
                        x_Start = boundingBox2D.Min.X;
                        factor_direction = 1;
                        break;

                    case Corner.TopRight:
                    case Corner.BottomRight:
                        x_Start = boundingBox2D.Max.X;
                        factor_direction = -1;
                        break;
                }

                double offset = verticalOffset.Value;
                int count = System.Convert.ToInt32(boundingBox2D.Width / offset);

                if (!includeEdge)
                    count--;
                else
                    count++;

                offset = offset * factor_direction;
                for (int i = 0; i < count; i++)
                {
                    int index = i;
                    if (!includeEdge)
                        index++;

                    double x = x_Start + (offset * index);
                    result.Add(new Segment2D(new Point2D(x, y_Start), new Point2D(x, y_End)));
                }
            }

            return result;
        }

        public static List<Segment2D> Segment2Ds(this IEnumerable<Point2D> point2Ds, bool close = false)
        {
            if (point2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            if (point2Ds.Count() < 2)
                return result;

            int aCount = point2Ds.Count();

            for (int i = 0; i < aCount - 1; i++)
                result.Add(new Segment2D(point2Ds.ElementAt(i), point2Ds.ElementAt(i + 1)));

            if (close)
                result.Add(new Segment2D(point2Ds.Last(), point2Ds.First()));

            return result;
        }

        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, int count)
        {
            if (count == -1)
                return null;

            List<Point2D> point2Ds_Start = Point2Ds(boundingBox2D, count);
            List<Point2D> point2Ds_End = Point2Ds(boundingBox2D, count);

            List<Segment2D> result = new List<Segment2D>();
            for (int i = 0; i < count; i++)
            {
                result.Add(new Segment2D(point2Ds_Start[0], point2Ds_End[0]));
            }

            return result;
        }

        public static List<Segment2D> Segment2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxDistance, bool unconnectedOnly = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();

            List<Tuple<BoundingBox2D, Segment2D>> tuples_All = new List<Tuple<BoundingBox2D, Segment2D>>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                foreach (Segment2D segment2D in segment2Ds_Temp)
                {
                    BoundingBox2D boundingBox = segment2D.GetBoundingBox(tolerance);
                    tuples_All.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox, segment2D));

                    segment2Ds.Add(segment2D);
                }
            }

            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Tuple<BoundingBox2D, Segment2D>> tuples_Extensions = new List<Tuple<BoundingBox2D, Segment2D>>();

            foreach (Segment2D segment2D in segment2Ds)
            {
                Segment2D segment2D_Temp = null;
                BoundingBox2D boundingBox_Temp = null;

                Vector2D vector2D = segment2D.Direction * maxDistance;

                Point2D point2D_Start = segment2D.GetStart();
                List<Segment2D> segment2Ds_Start = segment2Ds.FindAll(x => point2D_Start.AlmostEquals(x.GetStart(), tolerance) || point2D_Start.AlmostEquals(x.GetEnd(), tolerance));
                if(segment2Ds_Start.Count == 1 || !unconnectedOnly)
                {
                    segment2D_Temp = new Segment2D(point2D_Start, point2D_Start.GetMoved(vector2D.GetNegated()));
                    boundingBox_Temp = segment2D_Temp.GetBoundingBox(tolerance);
                    tuples_Extensions.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));
                    tuples_All.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));
                }

                Point2D point2D_End = segment2D.GetEnd();
                List<Segment2D> segment2Ds_End = segment2Ds.FindAll(x => point2D_End.AlmostEquals(x.GetStart(), tolerance) || point2D_End.AlmostEquals(x.GetEnd(), tolerance));
                if (segment2Ds_End.Count == 1 || !unconnectedOnly)
                {
                    segment2D_Temp = new Segment2D(point2D_End, point2D_End.GetMoved(vector2D));
                    boundingBox_Temp = segment2D_Temp.GetBoundingBox(tolerance);
                    tuples_Extensions.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));
                    tuples_All.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));
                }
            }

            for (int i = 0; i < tuples_Extensions.Count - 1; i++)
            {
                for (int j = i + 1; j < tuples_Extensions.Count; j++)
                {
                    if(tuples_Extensions[i].Item1.InRange(tuples_Extensions[j].Item1, tolerance))
                    {
                        Segment2D segment2D_1 = tuples_Extensions[i].Item2;
                        Segment2D segment2D_2 = tuples_Extensions[j].Item2;

                        if(!segment2D_1.Direction.SameHalf(segment2D_2.Direction))
                        {
                            if (segment2D_1.Collinear(segment2D_2, tolerance) && (segment2D_1.On(segment2D_2[0], tolerance) || segment2D_1.On(segment2D_2[1], tolerance)))
                            {
                                List<Point2D> point2Ds = new List<Point2D>() { segment2D_1[0], segment2D_1[1], segment2D_2[0], segment2D_2[1] };
                                point2Ds.RemoveAll(x => !segment2D_1.On(x, tolerance) || !segment2D_2.On(x, tolerance));
                                if(point2Ds.Count >= 2)
                                {
                                    Query.ExtremePoints(point2Ds, out Point2D point2D_1, out Point2D point2D_2);
                                    if (point2D_1 != null && point2D_2 != null && point2D_1.Distance(point2D_2) >= tolerance)
                                    {
                                        segment2Ds.Add(new Segment2D(point2D_1, point2D_2));
                                    }
                                }

                            }
                        }
                    }
                }
            }

            List<Segment2D> segment2Ds_Extension = Enumerable.Repeat<Segment2D>(null, tuples_Extensions.Count).ToList();

            Parallel.For(0, segment2Ds_Extension.Count, (int i) =>
            {
                Tuple<BoundingBox2D, Segment2D> tuple_Extension = tuples_Extensions[i];

                double distance = double.MaxValue;
                Point2D point2D = null;

                foreach (Tuple<BoundingBox2D, Segment2D> tuple in tuples_All)
                {
                    if (tuple_Extension.Item2 == tuple.Item2 || !tuple_Extension.Item1.InRange(tuple.Item1))
                    {
                        continue;
                    }

                    Point2D point2D_Intersection = tuple_Extension.Item2.Intersection(tuple.Item2, true, tolerance);
                    if (point2D_Intersection == null)
                    {
                        continue;
                    }

                    double distance_Temp = tuple_Extension.Item2[0].Distance(point2D_Intersection);
                    if (distance_Temp > tolerance && distance_Temp < distance)
                    {
                        distance = distance_Temp;
                        point2D = point2D_Intersection;
                    }
                }

                if (point2D == null || point2D.AlmostEquals(tuple_Extension.Item2[0], tolerance))
                {
                    return;
                }

                segment2Ds_Extension[i] = new Segment2D(tuple_Extension.Item2[0], point2D);
            });

            foreach(Segment2D segment2D_Extension in segment2Ds_Extension)
            {
                if(segment2D_Extension != null)
                {
                    segment2Ds.Add(segment2D_Extension);
                }
            }

            segment2Ds = segment2Ds.Split(tolerance);

            //segment2Ds = segment2Ds.Snap(true, snapTolerance);

            return segment2Ds;
        }
    }
}