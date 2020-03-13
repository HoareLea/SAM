using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D Normal(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.MicroDistance)
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

            for (int i = 1; i < aCount - 1; i++)
            {
                Point3D point3D_1 = point3Ds_Temp.ElementAt(i - 1);
                Point3D point3D_2 = point3Ds_Temp.ElementAt(i);
                Point3D point3D_3 = point3Ds_Temp.ElementAt(i + 1);

                Vector3D normal = Normal(point3D_1, point3D_2, point3D_3);
                if (normal.Length < tolerance)
                    continue;

                return normal.Unit;
            }

            return null;
        }

        public static Vector3D Normal(this Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            return new Vector3D(point3D_1, point3D_2).CrossProduct(new Vector3D(point3D_1, point3D_3));
        }

    }
}
