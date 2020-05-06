using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Vector3D> Normals(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Angle)
        {
            if (point3Ds == null)
                return null;

            int aCount = point3Ds.Count();
            if (aCount < 3)
                return null;

            List<Point3D> point3Ds_Temp = new List<Point3D>(point3Ds);
            point3Ds_Temp.Insert(0, point3Ds_Temp.Last());
            point3Ds_Temp.Add(point3Ds_Temp[1]);

            aCount = point3Ds_Temp.Count;

            List<Vector3D> result = new List<Vector3D>();
            for (int i = 1; i < aCount - 1; i++)
            {
                Point3D point3D_1 = point3Ds_Temp.ElementAt(i - 1);
                Point3D point3D_2 = point3Ds_Temp.ElementAt(i);
                Point3D point3D_3 = point3Ds_Temp.ElementAt(i + 1);

                Vector3D vector3D = null;
                if (!Collinear(point3D_1, point3D_2, point3D_3, tolerance))
                {
                    vector3D = Normal(point3D_1, point3D_2, point3D_3);
                    if (IsValid(vector3D))
                        vector3D = vector3D.Unit;
                    else
                        vector3D = null;
                }

                result.Add(vector3D);
            }

            return result;
        }
    }
}