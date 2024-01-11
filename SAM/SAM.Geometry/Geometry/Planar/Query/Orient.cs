using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> Orient(IEnumerable<Point2D> point2Ds, Orientation orientation)
        {
            if (point2Ds == null || point2Ds.Count() < 3 || orientation == Geometry.Orientation.Collinear)
                return null;

            List<Point2D> aResult = new List<Point2D>(point2Ds);

            if (orientation == Geometry.Orientation.Undefined)
                return aResult;

            List<Orientation> aOrienationList = Orientations(point2Ds);
            if (aOrienationList.Count(x => x == orientation) > (aOrienationList.Count / 2))
                return aResult;

            aResult.Reverse();

            return aResult;
        }
    }
}