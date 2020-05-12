using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Polygon2D ToSAM(this LinearRing linearRing, double tolerance = Core.Tolerance.Distance)
        {
            if (linearRing == null || linearRing.IsEmpty)
                return null;

            return new Polygon2D(linearRing.Coordinates.ToSAM(tolerance));
        }
    }
}