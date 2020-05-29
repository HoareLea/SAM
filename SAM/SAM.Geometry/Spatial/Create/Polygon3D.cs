using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

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

        public static Polygon3D Polygon3D(this Vector3D normal, IEnumerable<Point3D> point3Ds)
        {
            if (normal == null || point3Ds == null || point3Ds.Count() < 3)
                return null;

            Plane plane = new Plane(point3Ds.ElementAt(0), normal);

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Point3D point3D in point3Ds)
                point2Ds.Add(plane.Convert(plane.Project(point3D)));

            return new Polygon3D(plane, point2Ds);
        }
    }
}