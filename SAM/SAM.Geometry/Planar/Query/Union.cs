using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Union of the sets A and B, denoted A ∪ B, is the set of all objects that are a member of A, or B, or both. The union of {1, 2, 3} and {2, 3, 4} is the set {1, 2, 3, 4}
        public static List<Polygon2D> Union(this Polygon2D polygon2D_1, Polygon2D polygon2D_2)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            return new PointGraph2D(new List<Polygon2D>() { polygon2D_1, polygon2D_2 }, true).GetPolygon2Ds_External();
        }

        public static List<Polygon2D> Union(this IEnumerable<Polygon2D> polygon2Ds)
        {
            if (polygon2Ds == null)
                return null;

            int count = polygon2Ds.Count();

            List<Polygon2D> result = new List<Polygon2D>();

            if (count == 0)
                return result;

            if (count == 1)
            {
                result.Add(new Polygon2D(polygon2Ds.First()));
                return result;
            }

            return new PointGraph2D(polygon2Ds, true).GetPolygon2Ds_External();
        }
    }
}
