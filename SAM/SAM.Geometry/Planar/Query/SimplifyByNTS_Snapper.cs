using NetTopologySuite.Geometries;

using SAM.Core;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon SimplifyByNTS_Snapper(this Polygon polygon, double tolerance = Tolerance.Distance)
        {
            if (polygon == null)
                return null;

            Polygon result = NetTopologySuite.Operation.Overlay.Snap.GeometrySnapper.SnapToSelf(polygon, tolerance, true) as Polygon;
            if (result == null)
                return polygon;

            return result;
        }

        public static Polygon2D SimplifyByNTS_Snapper(this Polygon2D polygon2D, double tolerance = Tolerance.Distance)
        {
            if (polygon2D == null)
                return null;

            LinearRing linearRing = ((IClosed2D)polygon2D).ToNTS(tolerance);

            linearRing = NetTopologySuite.Operation.Overlay.Snap.GeometrySnapper.SnapToSelf(linearRing, tolerance, true) as LinearRing;
            if (linearRing == null)
                return null;

            return linearRing.ToSAM(tolerance);
        }

        public static Face2D SimplifyByNTS_Snapper(this Face2D face2D, double tolerance = Tolerance.Distance)
        {
            Polygon polygon = Convert.ToNTS(face2D as Face, tolerance);

            polygon = NetTopologySuite.Operation.Overlay.Snap.GeometrySnapper.SnapToSelf(polygon, tolerance, true) as Polygon;

            return polygon?.ToSAM(tolerance);
        }
    }
}