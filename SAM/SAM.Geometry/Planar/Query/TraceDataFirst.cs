using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Tuple<Point2D, Segment2D, Vector2D> TraceDataFirst(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds)
        {
            if (point2D == null || vector2D == null || segmentable2Ds == null)
                return null;

            List<Tuple<Point2D, Segment2D, Vector2D>> tuples = TraceData(point2D, vector2D, segmentable2Ds, 0);
            if (tuples == null || tuples.Count == 0)
                return null;

            return tuples.First();
        }

        public static Tuple<Point2D, Segment2D, Vector2D> TraceDataFirst(this Point2D point2D, Vector2D vector2D, ISegmentable2D segmentable2D)
        {
            if (point2D == null || vector2D == null || segmentable2D == null)
                return null;

            List<Tuple<Point2D, Segment2D, Vector2D>> tuples = TraceData(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, 0);
            if (tuples == null || tuples.Count == 0)
                return null;

            return tuples.First();
        }
    }
}