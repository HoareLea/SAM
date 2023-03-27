using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Point2D ToSAM(this Coordinate coordinate, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (coordinate == null)
                return null;

            return new Point2D(Core.Query.Round(coordinate.X, tolerance), Core.Query.Round(coordinate.Y, tolerance));
        }

        public static Point2D ToSAM(this Point point, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point == null || point.IsEmpty)
                return null;

            return new Point2D(Core.Query.Round(point.X, tolerance), Core.Query.Round(point.Y, tolerance));
        }

        public static Point2D ToSAM(this System.Numerics.Complex complex)
        {
            if(complex == null)
            {
                return null;
            }

            return new Point2D(complex.Real, complex.Imaginary);
        }
    }
}