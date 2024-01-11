using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static BoundingBox2D BoundingBox2D(this IEnumerable<ISegmentable2D> segmentable2Ds, double offset = 0)
        {
            if (segmentable2Ds == null)
                return null;

            int count = segmentable2Ds.Count();

            if (count == 0)
                return null;

            BoundingBox2D result = segmentable2Ds.ElementAt(0).GetBoundingBox(offset);

            if (count == 1)
                return result;

            for(int i =1; i < count; i++)
                result.Include(segmentable2Ds.ElementAt(i).GetBoundingBox(offset));

            return result;
        }
    }
}