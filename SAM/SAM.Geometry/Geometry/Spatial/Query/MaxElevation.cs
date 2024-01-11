using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double MaxElevation(this IEnumerable<Face3D> face3Ds)
        {
            if (face3Ds == null || face3Ds.Count() == 0)
                return double.NaN;

            double result = double.MinValue;
            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null)
                    continue;

                double maxElevation = MaxElevation(face3D);
                if (maxElevation > result)
                    result = maxElevation;
            }

            if (result == double.MinValue)
            {
                return double.NaN;
            }

            return result;
        }

        public static double MaxElevation(this Face3D face3D)
        {
            if(face3D == null)
            {
                return double.NaN;
            }
            
            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if (boundingBox3D == null)
                return double.NaN;

            return boundingBox3D.Max.Z;
        }
    }
}