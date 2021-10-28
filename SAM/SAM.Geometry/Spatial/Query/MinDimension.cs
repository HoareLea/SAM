using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double MinDimension(this BoundingBox3D boundingBox3D)
        {
            if(boundingBox3D == null)
            {
                return double.NaN;
            }

            return Math.Query.Min(boundingBox3D.Width, boundingBox3D.Height, boundingBox3D.Depth);
        }

        public static double MinDimension(this IEnumerable<BoundingBox3D> boundingBox3Ds)
        {
            if(boundingBox3Ds == null)
            {
                return double.NaN;
            }

            double result = double.MaxValue;
            foreach(BoundingBox3D boundingBox3D in boundingBox3Ds)
            {
                double min = boundingBox3D.MinDimension();
                if(min < result)
                {
                    result = min;
                }
            }

            return result;
        }
    }
}