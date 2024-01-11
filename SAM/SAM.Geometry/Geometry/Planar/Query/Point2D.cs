using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Point2D(this ISegmentable2D segmentable2D, double parameter, bool inverted = false)
        {
            if (parameter < 0 || parameter > 1 || segmentable2D == null)
                return null;

            List<Segment2D> segment2Ds = Trim(segmentable2D, parameter, inverted)?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            if (inverted)
                return segment2Ds.First().Start;
            else
                return segment2Ds.Last().End;
        }
    }
}