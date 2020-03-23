using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Vector2D> Trace(this Point2D point2D, Vector2D vector2D, IEnumerable<ISegmentable2D> segmentable2Ds, int bounces = 1)
        {
            if (point2D == null || vector2D == null || segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            HashSet<Point2D> point2Ds = new HashSet<Point2D>();
            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                if (segment2Ds_Temp == null)
                    continue;

                foreach(Segment2D segment2D in segment2Ds_Temp)
                {
                    if (segment2D == null)
                        continue;

                    segment2Ds.Add(segment2D);
                    point2Ds.Add(segment2D.GetStart());
                    point2Ds.Add(segment2D.GetEnd());
                }

                segment2Ds.AddRange(segment2Ds);
            }

            BoundingBox2D boundingBox2D = new BoundingBox2D(point2Ds, 1);

            List<Vector2D> result = new List<Vector2D>();


            return result;
        }
    }
}
