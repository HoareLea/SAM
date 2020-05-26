using NetTopologySuite.Geometries;
using SAM.Geometry.Spatial;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Face3D ToSAM(this Polygon polygon, Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon == null || polygon.IsEmpty || plane == null)
                return null;

            return new Face3D(plane, polygon.ToSAM(tolerance));
        }
    }
}