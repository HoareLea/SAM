using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static BoundingBox3D BoundingBox3D<T>(this IEnumerable<T> face3DObjects, double offset = 0) where T : IFace3DObject
        {
            if(face3DObjects == null || face3DObjects.Count() == 0)
            {
                return null;
            }

            List<BoundingBox3D> boundingBox3Ds = new List<BoundingBox3D>();
            foreach(T face3DObject in face3DObjects)
            {
                BoundingBox3D boundingBox3D = face3DObject?.GetBoundingBox(offset);
                if(boundingBox3D == null)
                {
                    continue;
                }

                boundingBox3Ds.Add(boundingBox3D);
            }

            if(boundingBox3Ds.Count == 0)
            {
                return null;
            }

            return new BoundingBox3D(boundingBox3Ds);
        }
    }
}