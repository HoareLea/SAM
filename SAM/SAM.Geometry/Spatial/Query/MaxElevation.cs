using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double MaxElevation(this IFace3DObject face3DObject)
        {
            BoundingBox3D boundingBox3D = face3DObject?.Face3D.GetBoundingBox();
            if (boundingBox3D == null)
                return double.NaN;

            return boundingBox3D.Max.Z;
        }

        public static double MaxElevation<T>(this IEnumerable<T> face3DObjects) where T: IFace3DObject
        {
            if (face3DObjects == null || face3DObjects.Count() == 0)
                return double.NaN;
            
            double result = double.MinValue;
            foreach(T face3DObject in face3DObjects)
            {
                if (face3DObject == null)
                    continue;

                double maxElevation = MaxElevation(face3DObject);
                if (maxElevation > result)
                    result = maxElevation;
            }

            return result;
        }
    }
}