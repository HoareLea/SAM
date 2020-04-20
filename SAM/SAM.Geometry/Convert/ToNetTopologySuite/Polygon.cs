using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Polygon ToNetTopologySuite_Polygon(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            return new Polygon(polygon2D?.ToNetTopologySuite(tolerance));
        }
    }
}
