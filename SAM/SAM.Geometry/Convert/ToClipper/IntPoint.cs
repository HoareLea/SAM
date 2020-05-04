using ClipperLib;
using NetTopologySuite.Geometries;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static IntPoint ToClipper(this Planar.Point2D point2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2D == null)
                return default;

            if (tolerance == 0)
                return new IntPoint(point2D.X, point2D.Y);

            return new IntPoint(point2D.X / tolerance, point2D.Y / tolerance);
        }

        public static IntPoint ToClipper(this Coordinate cooridnate, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (cooridnate == null)
                return default;

            if (tolerance == 0)
                return new IntPoint(cooridnate.X, cooridnate.Y);

            return new IntPoint(cooridnate.X / tolerance, cooridnate.Y / tolerance);
        }
    }
}