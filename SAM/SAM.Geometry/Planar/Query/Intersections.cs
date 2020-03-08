using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> Intersections(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2)
        {
            if (segmentable2D_1 == null || segmentable2D_2 == null)
                return null;

            List<Segment2D> segment2Ds_1 = segmentable2D_1.GetSegments();
            if (segment2Ds_1 == null)
                return null;

            List<Segment2D> segment2Ds_2= segmentable2D_2.GetSegments();
            if (segment2Ds_2 == null)
                return null;

            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach(Segment2D segment2D_1 in segment2Ds_1)
            {
                if (segment2D_1 == null)
                    continue;

                foreach(Segment2D segment2D_2 in segment2Ds_2)
                {
                    if (segment2D_2 == null)
                        continue;

                    Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, true);
                    if (point2D_Intersection == null)
                        continue;

                    point2Ds.Add(point2D_Intersection);
                }
            }

            return point2Ds.ToList();
        }

    }
}
