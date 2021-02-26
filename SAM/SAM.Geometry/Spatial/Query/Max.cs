using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Max(this IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() == 0)
                return null;

            double x = double.MinValue;
            double y = double.MinValue;
            double z = double.MinValue;
            foreach (Point3D point3D in point3Ds)
            {
                if (x < point3D.X)
                    x = point3D.X;
                if (y < point3D.Y)
                    y = point3D.Y;
                if (z < point3D.Z)
                    z = point3D.Z;
            }

            return new Point3D(x, y, z);
        }

        public static Point3D Max(this Point3D point3D_1, Point3D point3D_2)
        {
            return new Point3D(System.Math.Max(point3D_1.X, point3D_2.X), System.Math.Max(point3D_1.Y, point3D_2.Y), System.Math.Max(point3D_1.Z, point3D_2.Z));
        }
    }
}