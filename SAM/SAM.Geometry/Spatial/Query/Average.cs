using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Average(this IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null)
                return null;
            
            int count = point3Ds.Count();
            if (count < 1)
                return null;

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (Point3D point3D in point3Ds)
            {
                x = point3D.X;
                y = point3D.Y;
                z = point3D.Z;
            }

            return new Point3D(x / count, y / count, z / count);
        }
    }
}
