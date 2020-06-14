using ClipperLib;
using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Point ToNTS_Point(this Point2D point2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2D == null)
                return null;

            return new Point(Core.Query.Round(point2D.X, tolerance), Core.Query.Round(point2D.Y, tolerance));
        }
    }
}