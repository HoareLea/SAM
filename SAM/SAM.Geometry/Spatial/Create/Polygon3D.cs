using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Polygon3D Polygon3D(this IEnumerable<Point3D> point3Ds, double tolerace = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            Plane plane = Plane(point3Ds, tolerace);
            if (plane == null)
                return null;

            List<Point3D> point3Ds_Plane = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                if (point3D != null)
                    point3Ds_Plane.Add(plane.Project(point3D));

            return new Polygon3D(point3Ds_Plane);
        }
    }
}