using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        
        public static Plane Plane(this Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            if (point3D_1 == null || point3D_2 == null || point3D_3 == null)
            {
                return null;
            }
            
            
            Vector3D normal = Query.Normal(point3D_1, point3D_2, point3D_3);
            if (normal == null || !normal.IsValid())
            {
                return null;
            }

            return new Plane((new Point3D[] { point3D_1, point3D_2, point3D_3 }).Average(), normal);
        }
        
        public static Plane Plane(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            Vector3D normal = Query.Normal(point3Ds, tolerance);
            if (normal == null || !normal.IsValid())
                return null;

            return new Plane(point3Ds.Average(), normal);
        }

        public static Plane Plane(Point3D origin, Vector3D axisX, Vector3D axisY)
        {
            if (origin == null || axisX == null || axisY == null)
                return null;

            return new Plane(origin, axisX, axisY);
        }

        public static Plane Plane(double elevation)
        {
            return Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;
        }
    }
}