using System.Collections.Generic;


namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Polygon2D> Join(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                    segment2Ds.AddRange(segment2Ds_Temp);
            }

            segment2Ds.RemoveAll(x => x == null || x.GetLength() < tolerance);
            segment2Ds = Modify.Split(segment2Ds, tolerance);

            PointGraph2D pointGraph2D = new PointGraph2D(segment2Ds);
            return pointGraph2D.GetPolygon2Ds_External();
        }
    }
}
