using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Point3D> Clone(this IEnumerable<Point3D> point3Ds)
        {
            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                result.Add(new Point3D(point3D));

            return result;
        }
    }
}