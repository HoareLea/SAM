using NetTopologySuite.Geometries;

namespace SAM.Geometry.Planar
{
    public static partial class Convert
    {
        public static Polyline2D ToSAM(this LineString lineString, double tolerance = Core.Tolerance.Distance)
        {
            if (lineString == null || lineString.IsEmpty)
                return null;

            return new Polyline2D(lineString.Coordinates.ToSAM(tolerance), lineString.IsClosed);
        }
    }
}