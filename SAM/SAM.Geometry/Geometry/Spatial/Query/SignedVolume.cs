using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double SignedVolume(this Vector3D vector3D_1, Vector3D vector3D_2, Vector3D vector3D_3)
        {
            if(vector3D_1 == null || vector3D_2 == null || vector3D_3 == null)
            {
                return double.NaN;
            }
            
            double v321 = vector3D_3.X * vector3D_2.Y * vector3D_1.Z;
            double v231 = vector3D_2.X * vector3D_3.Y * vector3D_1.Z;
            double v312 = vector3D_3.X * vector3D_1.Y * vector3D_2.Z;
            double v132 = vector3D_1.X * vector3D_3.Y * vector3D_2.Z;
            double v213 = vector3D_2.X * vector3D_1.Y * vector3D_3.Z;
            double v123 = vector3D_1.X * vector3D_2.Y * vector3D_3.Z;

            return (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }

        public static double SignedVolume(this Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            if (point3D_1 == null || point3D_2 == null || point3D_3 == null)
            {
                return double.NaN;
            }

            double v321 = point3D_3.X * point3D_2.Y * point3D_1.Z;
            double v231 = point3D_2.X * point3D_3.Y * point3D_1.Z;
            double v312 = point3D_3.X * point3D_1.Y * point3D_2.Z;
            double v132 = point3D_1.X * point3D_3.Y * point3D_2.Z;
            double v213 = point3D_2.X * point3D_1.Y * point3D_3.Z;
            double v123 = point3D_1.X * point3D_2.Y * point3D_3.Z;

            return (1.0 / 6.0) * (-v321 + v231 + v312 - v132 - v213 + v123);
        }

        public static double SignedVolume(this Triangle3D triangle3D)
        {
            List<Point3D> point3Ds = triangle3D?.GetPoints();
            if(point3Ds == null || point3Ds.Count != 3)
            {
                return double.NaN;
            }

            return SignedVolume(point3Ds[0], point3Ds[1], point3Ds[2]);
        }
    }
}