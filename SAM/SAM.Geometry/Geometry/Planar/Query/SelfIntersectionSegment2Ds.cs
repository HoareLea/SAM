﻿using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //public static List<Segment2D> SelfIntersectionSegment2Ds(this Polygon2D polygon2D, double maxLength, double tolerance = Core.Tolerance.Distance)
        //{
        //    if (polygon2D == null)
        //        return null;

        //    List<Segment2D> segment2Ds = polygon2D.GetSegments();

        //    List<Segment2D> result = new List<Segment2D>();
        //    foreach (Segment2D segment2D in segment2Ds)
        //    {
        //        Point2D point2D = segment2D.GetStart();
        //        Vector2D vector2D = segment2D.Direction.GetNegated();

        //        Vector2D vector2D_Intersection = TraceFirst(point2D, vector2D, polygon2D);
        //        if (vector2D_Intersection != null && vector2D_Intersection.Length > 0)
        //        {
        //            Segment2D segment2D_Intersection = new Segment2D(point2D.GetMoved(vector2D_Intersection), point2D);
        //            if (segment2D_Intersection.GetLength() > maxLength)
        //                continue;

        //            List<Point2D> point2Ds_Intersections = polygon2D.Intersections(segment2D_Intersection, tolerance);
        //            if (point2Ds_Intersections != null && point2Ds_Intersections.Count == 2)
        //                if (result.Find(x => x.AlmostSimilar(segment2D_Intersection, tolerance)) == null)
        //                    result.Add(segment2D_Intersection);
        //        }

        //        point2D = segment2D.GetEnd();
        //        vector2D = segment2D.Direction;

        //        vector2D_Intersection = TraceFirst(point2D, vector2D, polygon2D);
        //        if (vector2D_Intersection != null && vector2D_Intersection.Length > 0)
        //        {
        //            Segment2D segment2D_Intersection = new Segment2D(point2D.GetMoved(vector2D_Intersection), point2D);
        //            if (segment2D_Intersection.GetLength() > maxLength)
        //                continue;

        //            List<Point2D> point2Ds_Intersections = polygon2D.Intersections(segment2D_Intersection, tolerance);
        //            if (point2Ds_Intersections != null && point2Ds_Intersections.Count == 2)
        //                if (result.Find(x => x.AlmostSimilar(segment2D_Intersection, tolerance)) == null)
        //                    result.Add(segment2D_Intersection);
        //        }
        //    }

        //    result.AddRange(segment2Ds);

        //    return result;
        //}

        public static List<Segment2D> SelfIntersectionSegment2Ds(this IEnumerable<Segment2D> segment2Ds, double maxLength, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null)
                return null;


            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                Point2D point2D = segment2D.GetStart();
                Vector2D vector2D = segment2D.Direction.GetNegated();

                List<Segment2D> segment2Ds_Temp = new List<Segment2D>(segment2Ds);
                segment2Ds_Temp.Remove(segment2D);

                Vector2D vector2D_Intersection = TraceFirst(point2D, vector2D, segment2Ds_Temp);
                if (vector2D_Intersection != null && vector2D_Intersection.Length > 0)
                {
                    Segment2D segment2D_Intersection = new Segment2D(point2D.GetMoved(vector2D_Intersection), point2D);
                    if (segment2D_Intersection.GetLength() > maxLength)
                        continue;

                    List<Point2D> point2Ds_Intersections = segment2D_Intersection.Intersections(segment2Ds_Temp, tolerance);
                    if (point2Ds_Intersections != null && point2Ds_Intersections.Count == 2)
                        if (result.Find(x => x.AlmostSimilar(segment2D_Intersection, tolerance)) == null)
                            result.Add(segment2D_Intersection);
                }

                point2D = segment2D.GetEnd();
                vector2D = segment2D.Direction;

                vector2D_Intersection = TraceFirst(point2D, vector2D, segment2Ds_Temp);
                if (vector2D_Intersection != null && vector2D_Intersection.Length > 0)
                {
                    Segment2D segment2D_Intersection = new Segment2D(point2D.GetMoved(vector2D_Intersection), point2D);
                    if (segment2D_Intersection.GetLength() > maxLength)
                        continue;

                    List<Point2D> point2Ds_Intersections = segment2D_Intersection.Intersections(segment2Ds_Temp, tolerance);
                    if (point2Ds_Intersections != null && point2Ds_Intersections.Count == 2)
                        if (result.Find(x => x.AlmostSimilar(segment2D_Intersection, tolerance)) == null)
                            result.Add(segment2D_Intersection);
                }
            }

            result.AddRange(segment2Ds);

            return result;
        }

        public static List<Segment2D> SelfIntersectionSegment2Ds(this ISegmentable2D segmentable2D, double maxLength, double tolerance = Core.Tolerance.Distance)
        {
            return SelfIntersectionSegment2Ds(segmentable2D?.GetSegments(), maxLength, tolerance);
        }

        public static List<Segment2D> SelfIntersectionSegment2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxLength, double tolerance = Core.Tolerance.Distance)
        {
            return SelfIntersectionSegment2Ds(segmentable2Ds?.Segment2Ds(), maxLength, tolerance);
        }
    }
}