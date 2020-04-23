using ClipperLib;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Polygon2D> Simplify(this Polygon2D polygon2D, double tolerance = Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            List<IntPoint> intPoints = ((ISegmentable2D)polygon2D).ToClipper(tolerance);
            if (intPoints == null || intPoints.Count == 0)
                return null;

            List<List<IntPoint>> intPointsList = Clipper.SimplifyPolygon(intPoints);
            if (intPointsList == null || intPointsList.Count == 0)
                return null;

            return intPointsList.ConvertAll(x => new Polygon2D(x.ToSAM(tolerance)));
        }
    }
}
