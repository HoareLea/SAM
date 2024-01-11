using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> AdjacentSegment2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = segmentable2Ds.Segment2Ds();
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            
            if (segment2Ds.Count < 2)
                return result;

            List<Segment2D> segment2Ds_Split = Split(segment2Ds, tolerance);
            if (segment2Ds_Split.Count < 2)
                return result;

            foreach(Segment2D segment2D in segment2Ds_Split)
            {
                Point2D point2D = segment2D?.Mid();
                if (point2D == null)
                    continue;

                List<Segment2D> segments2D_Temp = segment2Ds.FindAll(x => x.On(point2D, tolerance));
                if (segments2D_Temp == null)
                    continue;

                if (segments2D_Temp.Count > 1)
                    result.Add(segment2D);
            }

            return result;
        }
    }
}