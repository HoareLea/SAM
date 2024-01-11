using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Range<double>> Ranges(this Face3D face3D, IEnumerable<Range<double>> ranges, int dimensionIndex = 2)
        {
            if(face3D == null || ranges == null || ranges.Count() == 0)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            Range<double> range_BoundingBox3D = boundingBox3D.Range(dimensionIndex);

            List<Range<double>> result = new List<Range<double>>();
            foreach(Range<double> range in ranges)
            {
                if(range == null)
                {
                    continue;
                }

                if(!range_BoundingBox3D.Intersect(range))
                {
                    continue;
                }

                result.Add(range);
            }

            return result;
        }
    }
}