using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static ISAMGeometry2D ToSAM(this NetTopologySuite.Geometries.Geometry geometry, double tolerance = Core.Tolerance.Distance)
        {
            return Convert.ToSAM(geometry as dynamic, tolerance);
        }
    }
}