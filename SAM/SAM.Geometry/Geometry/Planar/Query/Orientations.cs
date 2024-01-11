using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Orientation> Orientations(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
                return null;

            int count = point2Ds.Count();

            if (count < 3)
                return null;

            List<Orientation> result = new List<Orientation>();

            result.Add(Orientation(point2Ds.ElementAt(count - 1), point2Ds.ElementAt(0), point2Ds.ElementAt(1)));

            for (int i = 1; i < count - 1; i++)
                result.Add(Orientation(point2Ds.ElementAt(i - 1), point2Ds.ElementAt(i), point2Ds.ElementAt(i + 1)));

            result.Add(Orientation(point2Ds.ElementAt(count - 2), point2Ds.ElementAt(count - 1), point2Ds.ElementAt(0)));

            return result;
        }
    }
}