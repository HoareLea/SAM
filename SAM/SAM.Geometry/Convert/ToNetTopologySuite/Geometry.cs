using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static NetTopologySuite.Geometries.Geometry ToNetTopologySuite(this ISAMGeometry2D sAMGeometry2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (sAMGeometry2D == null)
                return null;
            
            return ToNetTopologySuite(sAMGeometry2D as dynamic, tolerance);
        }
    }
}
