using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<T> Intersections<T>(this IEnumerable<Face3D> face3Ds, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T : ISAMGeometry3D
        {
            if (face3Ds == null)
                return null;

            int count = face3Ds.Count();

            List<T> result = new List<T>();
            if (count < 2)
                return result;

            for(int i =0; i < count - 1; i++)
            {
                Face3D face3D_1 = face3Ds.ElementAt(i);
                if (face3D_1 == null)
                    continue;

                for (int j = i + 1; j < count; j++)
                {
                    Face3D face3D_2 = face3Ds.ElementAt(j);
                    if (face3D_2 == null)
                        continue;

                    PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D_1, face3D_2, tolerance_Angle, tolerance_Distance);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                        continue;

                    List<T> geometry3Ds = planarIntersectionResult.GetGeometry3Ds<T>();
                    if (geometry3Ds != null && geometry3Ds.Count > 0)
                        result.AddRange(geometry3Ds);
                }
            }

            return result;
        }
    }
}