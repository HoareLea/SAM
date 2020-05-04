using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Vector2D> Trace(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds, int bounces = 0)
        {
            if (point2D == null || vector2D == null || segmentable2Ds == null)
                return null;

            return TraceData(point2D, vector2D, segmentable2Ds, bounces)?.ConvertAll(x => x.Item3);
        }

        public static List<Vector2D> Trace(this Point2D point2D, Vector2D vector2D, ISegmentable2D segmentable2D, int bounces = 0)
        {
            if (point2D == null || vector2D == null || segmentable2D == null)
                return null;

            return TraceData(point2D, vector2D, new ISegmentable2D[] { segmentable2D }, bounces)?.ConvertAll(x => x.Item3);
        }
    }
}