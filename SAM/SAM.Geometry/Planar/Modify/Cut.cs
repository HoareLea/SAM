using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Polygon2D> Cut(this Polygon2D polygon2D_ToBeCut, Polygon2D polygon)
        {
            if (polygon2D_ToBeCut == null || polygon == null)
                return null;

            List<Polygon2D> polygon2Ds = new PointGraph2D(new Polygon2D[] { polygon2D_ToBeCut, polygon }, true).GetPolygon2Ds();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            foreach(Polygon2D polygon2D_Temp in polygon2Ds)
            {
                if (!polygon.Inside(polygon2D_Temp.GetInternalPoint2D()))
                    result.Add(polygon2D_Temp);
            }

            return result;
        }
    }
}
