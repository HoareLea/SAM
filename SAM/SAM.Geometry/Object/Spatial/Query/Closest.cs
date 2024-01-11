using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static T Closest<T>(this IEnumerable<T> face3DObjects, Point3D point3D) where T : IFace3DObject
        {
            if (face3DObjects == null)
            {
                return default;
            }

            Dictionary<Face3D, T> dictionary = new Dictionary<Face3D, T>();
            foreach (T face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                dictionary[face3D] = face3DObject;
            }

            Face3D face3D_Closest = Geometry.Spatial.Query.Closest(dictionary.Keys, point3D);
            if (face3D_Closest == null)
            {
                return default;
            }

            return dictionary[face3D_Closest];
        }
    }
}