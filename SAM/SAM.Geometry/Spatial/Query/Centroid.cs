using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Centroid(this IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() < 3)
                return null;

            Vector3D vector3D = new Vector3D();
            double area = 0;

            Point3D point3D_1 = point3Ds.ElementAt(0);
            Point3D point3D_2 = point3Ds.ElementAt(1);

            for (var i = 2; i < point3Ds.Count(); i++)
            {
                Point3D point3D_3 = point3Ds.ElementAt(i);
                Vector3D vector3D_1 = point3D_3 - point3D_1;
                Vector3D vector3D_2 = point3D_3 - point3D_2;

                Vector3D vector3D_3 = vector3D_1.CrossProduct(vector3D_2);
                double area_Temp = vector3D_3.Length / 2;

                vector3D.X += area_Temp * (point3D_1.X + point3D_2.X + point3D_3.X) / 3;
                vector3D.Y += area_Temp * (point3D_1.Y + point3D_2.Y + point3D_3.Y) / 3;
                vector3D.Z += area_Temp * (point3D_1.Z + point3D_2.Z + point3D_3.Z) / 3;

                area += area_Temp;
                point3D_2 = point3D_3;
            }

            if (area == 0)
                return null;

            return new Point3D
            {
                X = vector3D.X / area,
                Y = vector3D.Y / area,
                Z = vector3D.Z / area
            };
        }
    }
}