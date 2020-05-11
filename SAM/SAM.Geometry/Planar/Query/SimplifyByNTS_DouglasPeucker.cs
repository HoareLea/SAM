using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using SAM.Core;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon SimplifyByNTS_DouglasPeucker(this Polygon polygon, double tolerance = Tolerance.Distance)
        {
            if (polygon == null)
                return null;

            Polygon result = DouglasPeuckerSimplifier.Simplify(polygon, tolerance) as Polygon;
            if (result == null)
                return polygon;

            return result;
        }
    }
}