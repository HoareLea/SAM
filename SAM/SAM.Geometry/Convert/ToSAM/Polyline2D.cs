using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Polyline2D ToSAM(this LineString lineString, double tolerance = Core.Tolerance.Distance)
        {
            if (lineString == null)
                return null;

            return new Polyline2D(lineString.Coordinates.ToSAM(tolerance), lineString.IsClosed);
        }
    }
}