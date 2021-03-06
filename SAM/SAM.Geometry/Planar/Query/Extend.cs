﻿using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> Extend(this IEnumerable<Segment2D> segment2Ds, ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || segmentable2D == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();

            if (segment2Ds.Count() == 0)
                return result;

            foreach (Segment2D segment2D in segment2Ds)
            {
                if (segment2D == null)
                    continue;

                Segment2D segment2D_Temp = Extend(segment2D, segmentable2D, tolerance);
                if (segment2D_Temp == null)
                    continue;

                result.Add(segment2D_Temp);
            }

            return result;
        }

        public static Segment2D Extend(this Segment2D segment2D, ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null || segmentable2D == null)
                return null;

            Point2D point2D_Segment2D_Start = segment2D.Start;
            Point2D point2D_Segment2D_End = segment2D.End;

            Vector2D vector2D;

            Point2D point2D_Start = null;

            vector2D = TraceFirst(point2D_Segment2D_Start, segment2D.Direction.GetNegated(), segmentable2D);
            if (vector2D != null)
                point2D_Start = point2D_Segment2D_Start.GetMoved(vector2D);

            if (point2D_Start == null && segmentable2D.On(point2D_Segment2D_Start, tolerance))
                point2D_Start = point2D_Segment2D_Start;

            Point2D point2D_End = null;

            vector2D = TraceFirst(point2D_Segment2D_End, segment2D.Direction, segmentable2D);
            if (vector2D != null)
                point2D_End = point2D_Segment2D_End.GetMoved(vector2D);

            if (point2D_End == null && segmentable2D.On(point2D_Segment2D_End, tolerance))
                point2D_End = point2D_Segment2D_End;

            if (point2D_Start == null && point2D_End == null)
                return null;

            if (point2D_Start == null)
                point2D_Start = point2D_Segment2D_Start;

            if (point2D_End == null)
                point2D_End = point2D_Segment2D_End;

            return new Segment2D(point2D_Start, point2D_End);
        }

        public static Segment2D Extend(this Segment2D segment2D, double distance, bool extendStart = true, bool extendEnd = true)
        {
            if (segment2D == null)
                return null;

            if (!extendStart && !extendEnd)
                return new Segment2D(segment2D);

            if (distance == 0)
                return new Segment2D(segment2D);

            Vector2D vector2D = segment2D.Direction * distance;

            Point2D point2D_End = segment2D.End;
            if (extendEnd)
                point2D_End.Move(vector2D);

            vector2D.Negate();

            Point2D point2D_Start = segment2D.Start;
            if (extendStart)
                point2D_Start.Move(vector2D);

            return new Segment2D(point2D_Start, point2D_End);
        }

        public static Polyline2D Extend(this Polyline2D polyline2D, ISegmentable2D segmentable2D, bool extendStart = true, bool extendEnd = true)
        {
            if (polyline2D == null || segmentable2D == null)
                return null;

            if (polyline2D.IsClosed() || (!extendEnd && !extendStart))
                return new Polyline2D(polyline2D);

            List<Segment2D> segment2Ds = polyline2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            if (extendStart)
            {
                Segment2D segment2D = segment2Ds[0];

                Point2D point2D = segment2D.Start;
                Vector2D vector2D = segment2D.Direction;
                vector2D.Negate();

                List<Vector2D> vector2Ds = Trace(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, 0);
                if (vector2Ds != null && vector2Ds.Count > 0)
                    segment2Ds[0] = new Segment2D(point2D.GetMoved(vector2Ds[0]), segment2D.End);
            }

            if (extendEnd)
            {
                Segment2D segment2D = segment2Ds[segment2Ds.Count - 1];

                Point2D point2D = segment2D.End;
                Vector2D vector2D = segment2D.Direction;

                List<Vector2D> vector2Ds = Trace(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, 0);
                if (vector2Ds != null && vector2Ds.Count > 0)
                    segment2Ds[segment2Ds.Count - 1] = new Segment2D(segment2D.End, point2D.GetMoved(vector2Ds[0]));
            }

            return new Polyline2D(segment2Ds);
        }
    }
}