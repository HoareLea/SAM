using System.Linq;
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
    }
}
