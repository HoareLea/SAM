using ClipperLib;
using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Coordinate ToNTS(this Point2D point2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2D == null)
                return null;

            return new Coordinate(Core.Query.Round(point2D.X, tolerance), Core.Query.Round(point2D.Y, tolerance));
        }

        public static Coordinate ToNTS(this IntPoint intPoint, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (intPoint == null)
                return null;

            return new Coordinate(intPoint.X * tolerance, intPoint.Y * tolerance);
        }
    }
}