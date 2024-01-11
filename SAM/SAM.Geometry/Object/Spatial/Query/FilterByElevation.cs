using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static List<T> FilterByElevation<T>(this IEnumerable<T> face3DObjects, double elevation, out List<T> lower, out List<T> upper, double tolerance = Core.Tolerance.Distance) where T: IFace3DObject
        {
            lower = null;
            upper = null;

            if (face3DObjects == null)
                return null;

            lower = new List<T>();
            upper = new List<T>();

            List<T> result = new List<T>();
            foreach(T face3DObject in face3DObjects)
            {
                BoundingBox3D boundingBox3D = face3DObject?.Face3D.GetBoundingBox();
                if(boundingBox3D == null)
                {
                    continue;
                }
                
                double min = boundingBox3D.Min.Z;
                double max = boundingBox3D.Max.Z;

                if (min - tolerance <= elevation && max + tolerance >= elevation)
                {
                    if (System.Math.Abs(max - min) > tolerance && System.Math.Abs(max - elevation) < tolerance)
                    {
                        lower.Add(face3DObject);
                        continue;
                    }


                    result.Add(face3DObject);
                }
                else
                {
                    if (min >= elevation)
                        upper.Add(face3DObject);
                    else
                        lower.Add(face3DObject);
                }
            }

            return result;
        }
    }
}