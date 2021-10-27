using SAM.Core;
using System.Collections.Generic;
using SAM.Geometry.Spatial;


namespace SAM.Geometry
{
    public static partial class Query
    {
        public static Dictionary<double, List<T>> MinElevationDictionary<T>(this IEnumerable<T> face3DObjects, double tolerance = Tolerance.MicroDistance) where T: IFace3DObject
        {
            if (face3DObjects == null)
                return null;

            Dictionary<double, List<T>> result = new Dictionary<double, List<T>>();
            foreach (T face3DObject in face3DObjects)
            {               
                double minElevation = Core.Query.Round(face3DObject.MinElevation(), tolerance);

                if (!result.TryGetValue(minElevation, out List<T> face3DObjects_Elevation))
                {
                    face3DObjects_Elevation = new List<T>();
                    result[minElevation] = face3DObjects_Elevation;
                }

                face3DObjects_Elevation.Add(face3DObject);
            }

            return result;
        }
    }
}