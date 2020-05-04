using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Polyline2D ToSAM(this LineString lineString)
        {
            if (lineString == null)
                return null;

            return new Polyline2D(lineString.Coordinates.ToSAM(), lineString.IsClosed);
        }
    }
}