using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool IsValid(this Vector3D vector3D)
        {
            if (vector3D == null)
            {
                return false;
            }
                
            double x = vector3D.X;
            double y = vector3D.Y;
            double z = vector3D.Z;

            if (double.IsNaN(x) || double.IsNaN(x) || double.IsNaN(x))
            {
                return false;
            }

            if (x == 0 && y == 0 && z == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(Point3D point3D)
        {
            if (point3D == null)
                return false;

            double x = point3D.X;
            double y = point3D.Y;
            double z = point3D.Z;

            if (double.IsNaN(x) || double.IsNaN(x) || double.IsNaN(x))
                return false;

            return true;
        }

        public static bool IsValid(this Plane plane)
        {
            if(plane == null)
            {
                return false;
            }

            if(!IsValid(plane.Origin))
            {
                return false;
            }

            if(!IsValid(plane.Normal))
            {
                return false;
            }

            if (!IsValid(plane.AxisY))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Polygon3D polygon3D)
        {
            if (polygon3D == null)
            {
                return false;
            }

            Plane plane = polygon3D.GetPlane();
            if(!IsValid(plane))
            {
                return false;
            }

            List<Point3D> point3Ds = polygon3D.GetPoints();
            if(point3Ds == null || point3Ds.Count < 3)
            {
                return false;
            }

            foreach(Point3D point3D in point3Ds)
            {
                if(!IsValid(point3D))
                {
                    return false;
                }
            }

            return true;
        }
    }
}