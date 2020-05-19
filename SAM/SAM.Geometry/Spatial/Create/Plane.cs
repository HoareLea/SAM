using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Plane Plane(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            Vector3D normal = Query.Normal(point3Ds, tolerance);
            if (normal == null)
                return null;

            return new Plane(point3Ds.Average(), normal);
        }

        public static Plane Plane(Point3D origin, Vector3D direction_X, Vector3D direction_Y)
        {
            if (origin == null || direction_X == null || direction_Y == null)
                return null;

            return new Plane(origin, direction_X, direction_Y);
        }
    }
}