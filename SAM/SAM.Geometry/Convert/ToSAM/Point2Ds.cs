using System.Collections.Generic;

using ClipperLib;
using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<Point2D> ToSAM(this IEnumerable<IntPoint> intPoints, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (intPoints == null)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (IntPoint intPoint in intPoints)
                point2Ds.Add(intPoint.ToSAM(tolerance));

            return point2Ds;
        }
    }
}
