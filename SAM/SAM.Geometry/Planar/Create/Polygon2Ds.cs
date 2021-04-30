using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                    segment2Ds.AddRange(segment2Ds_Temp);
            }

            List<Polygon> polygons = segment2Ds.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            foreach (Polygon polygon in polygons)
            {
                List<Polygon2D> polygon2Ds = polygon.ToSAM_Polygon2Ds();
                if (polygon2Ds == null)
                    continue;

                //result.AddRange(polygon2Ds);

                //Removing duplicated polygon2Ds
                foreach (Polygon2D polygon2D in polygon2Ds)
                    if (result.Find(x => x.Similar(polygon2D)) == null)
                        result.AddRange(polygon2Ds);
            }

            return result;
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, bool split, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            if (split)
                return Polygon2Ds(segmentable2Ds.Split(tolerance), tolerance);

            return Polygon2Ds(segmentable2Ds, tolerance);
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxDistance, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();

            List<Tuple<BoundingBox2D, Segment2D>> tuples_Extensions = new List<Tuple<BoundingBox2D, Segment2D>>();
            List<Tuple<BoundingBox2D, Segment2D>> tupless_All = new List<Tuple<BoundingBox2D, Segment2D>>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                foreach (Segment2D segment2D in segment2Ds_Temp)
                {
                    Vector2D vector2D = segment2D.Direction * maxDistance;

                    BoundingBox2D boundingBox = segment2D.GetBoundingBox(tolerance);
                    tupless_All.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox, segment2D));

                    segment2Ds.Add(segment2D);

                    Segment2D segment2D_Temp = null;
                    BoundingBox2D boundingBox_Temp = null;

                    segment2D_Temp = new Segment2D(segment2D[1], segment2D[1].GetMoved(vector2D));
                    boundingBox_Temp = segment2D_Temp.GetBoundingBox(tolerance);
                    tuples_Extensions.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));
                    tupless_All.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));

                    segment2D_Temp = new Segment2D(segment2D[0], segment2D[0].GetMoved(vector2D.GetNegated()));
                    boundingBox_Temp = segment2D_Temp.GetBoundingBox(tolerance);
                    tuples_Extensions.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));
                    tupless_All.Add(new Tuple<BoundingBox2D, Segment2D>(boundingBox_Temp, segment2D_Temp));
                }
            }

            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            foreach (Tuple<BoundingBox2D, Segment2D> tuple_Extension in tuples_Extensions)
            {
                double distance = double.MaxValue;
                Point2D point2D = null;

                foreach (Tuple<BoundingBox2D, Segment2D> tuple in tupless_All)
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

                if (point2D == null || point2D.AlmostEquals(tuple_Extension.Item2[0]))
                {
                    continue;
                }

                segment2Ds.Add(new Segment2D(tuple_Extension.Item2[0], point2D));
            }

            segment2Ds = segment2Ds.Split(tolerance);

            segment2Ds = segment2Ds.Snap(true, snapTolerance);

            return Polygon2Ds(segment2Ds, snapTolerance);
        }
    }
}