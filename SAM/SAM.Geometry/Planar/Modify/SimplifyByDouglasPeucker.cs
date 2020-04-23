using ClipperLib;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static Polygon SimplifyByDouglasPeucker(this Polygon polygon, double tolerance = Tolerance.Distance)
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
