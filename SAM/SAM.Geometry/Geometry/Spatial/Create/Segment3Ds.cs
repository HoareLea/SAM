using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Segment3D> Segment3Ds(this IEnumerable<Point3D> point3Ds, bool close = false)
        {
            if (point3Ds == null)
                return null;

            List<Segment3D> result = new List<Segment3D>();
            if (point3Ds.Count() < 2)
                return result;

            int aCount = point3Ds.Count();

            for (int i = 0; i < aCount - 1; i++)
                result.Add(new Segment3D(point3Ds.ElementAt(i), point3Ds.ElementAt(i + 1)));

            if (close)
                result.Add(new Segment3D(point3Ds.Last(), point3Ds.First()));

            return result;
        }
    }
}