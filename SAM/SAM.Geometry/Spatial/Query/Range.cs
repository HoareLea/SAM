using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Range<double> Range(this BoundingBox3D boundingBox3D, int dimensionIndex)
        {
            if(boundingBox3D == null)
            {
                return null;
            }

            return new Range<double>(boundingBox3D.Min[dimensionIndex], boundingBox3D.Max[dimensionIndex]);
        }
    }
}