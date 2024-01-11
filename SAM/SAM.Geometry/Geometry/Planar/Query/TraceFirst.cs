using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Vector2D TraceFirst(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds)
        {
            if (point2D == null || vector2D == null || segmentable2Ds == null)
                return null;

            return TraceDataFirst(point2D, vector2D, segmentable2Ds)?.Item3;
        }

        public static Vector2D TraceFirst(this Point2D point2D, Vector2D vector2D, ISegmentable2D segmentable2D)
        {
            if (point2D == null || vector2D == null || segmentable2D == null)
                return null;

            return TraceFirst(point2D, vector2D, new ISegmentable2D[] { segmentable2D });
        }
    }
}