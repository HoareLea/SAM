using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using SAM.Core;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Face3D SimplifyByNTS_TopologyPreservingSimplifier(this Face3D face3D, double tolerance = Tolerance.Distance)
        {
            if (face3D == null)
                return null;

            Plane plane = face3D.GetPlane();
            if (plane == null)
                return null;

            Polygon polygon = face3D.ToNTS(tolerance);

            polygon = TopologyPreservingSimplifier.Simplify(polygon, tolerance) as Polygon;
            if (polygon.Area <= tolerance)
                return null;

            return polygon?.ToSAM(plane, tolerance);
        }
    }
}