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

            Polygon polygon = face3D.ToNTS(tolerance);

            polygon = TopologyPreservingSimplifier.Simplify(polygon, tolerance) as Polygon;

            return polygon?.ToSAM(face3D.GetPlane(), tolerance);
        }
    }
}