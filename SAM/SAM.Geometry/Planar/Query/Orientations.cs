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

            int aCount = point2Ds.Count();

            if (aCount < 3)
                return null;

            List<Orientation> aResult = new List<Orientation>();

            aResult.Add(Orientation(point2Ds.ElementAt(aCount - 1), point2Ds.ElementAt(0), point2Ds.ElementAt(1)));

            for (int i = 1; i < aCount - 1; i++)
                aResult.Add(Orientation(point2Ds.ElementAt(i - 1), point2Ds.ElementAt(i), point2Ds.ElementAt(i + 1)));

            aResult.Add(Orientation(point2Ds.ElementAt(aCount - 2), point2Ds.ElementAt(aCount - 1), point2Ds.ElementAt(0)));

            return aResult;
        }
    }
}