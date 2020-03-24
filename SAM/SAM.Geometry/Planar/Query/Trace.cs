using System.Collections.Generic;
using System.Linq;

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
    }
}
