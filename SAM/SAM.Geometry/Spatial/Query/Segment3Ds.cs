using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Segment3D> Segment3Ds<T>(this IEnumerable<T> segmentable3Ds) where T :ISegmentable3D
        {
            if(segmentable3Ds == null)
            {
                return null;
            }

            List<Segment3D> result = new List<Segment3D>();
            foreach(ISegmentable3D segmentable3D in segmentable3Ds)
            {
                List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
                if(segment3Ds == null)
                {
                    continue;
                }

                foreach(Segment3D segment3D in segment3Ds)
                {
                    if(segment3D == null)
                    {
                        continue;
                    }

                    result.Add(segment3D);
                }
            }

            return result;
        }
    }
}