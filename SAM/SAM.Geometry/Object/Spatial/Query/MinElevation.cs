using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static double MinElevation(this IFace3DObject face3DObject)
        {
            BoundingBox3D boundingBox3D = face3DObject?.Face3D.GetBoundingBox();
            if (boundingBox3D == null)
                return double.NaN;

            return boundingBox3D.Min.Z;
        }

        public static double MinElevation<T>(this IEnumerable<T> face3DObjects) where T: IFace3DObject
        {
            if (face3DObjects == null || face3DObjects.Count() == 0)
                return double.NaN;
            
            double result = double.MaxValue;
            foreach(T face3DObject in face3DObjects)
            {
                if (face3DObject == null)
                    continue;

                double minElevation = MinElevation(face3DObject);
                if (minElevation < result)
                    result = minElevation;
            }

            if(result == double.MaxValue)
            {
                return double.NaN;
            }

            return result;
        }
    }
}