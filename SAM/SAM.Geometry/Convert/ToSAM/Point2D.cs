using ClipperLib;

using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Point2D ToSAM(this IntPoint intPoint, double tolerance = Core.Tolerance.MicroDistance)
        {
            return new Point2D(intPoint.X * tolerance, intPoint.Y * tolerance);
        }

        public static Point2D ToSAM(this Coordinate coordinate)
        {
            return new Point2D(coordinate.X, coordinate.Y);
        }

        public static Point2D ToSAM(this Point point)
        {
            return new Point2D(point.X, point.Y);
        }
    }
}
