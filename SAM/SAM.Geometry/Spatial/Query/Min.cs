using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Min(this Point3D point2D_1, Point3D point2D_2)
        {
            return new Point3D(System.Math.Min(point2D_1.X, point2D_2.X), System.Math.Min(point2D_1.Y, point2D_2.Y), System.Math.Min(point2D_1.Z, point2D_2.Z));
        }

        public static Point3D Min(this IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() == 0)
                return null;

            double x = double.MaxValue;
            double y = double.MaxValue;
            double z = double.MaxValue;
            foreach (Point3D point3D in point3Ds)
            {
                if (x > point3D.X)
                    x = point3D.X;
                if (y > point3D.Y)
                    y = point3D.Y;
                if (z > point3D.Z)
                    z = point3D.Z;
            }

            return new Point3D(x, y, z);
        }
    }
}