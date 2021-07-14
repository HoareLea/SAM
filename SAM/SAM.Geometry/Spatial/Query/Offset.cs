using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Offset(this Face3D face3D, double offset, bool includeExternalEdge = true, bool includeInternalEdges = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null || !plane.IsValid())
                return null;

            List<Face2D> face2Ds = plane.Convert(face3D)?.Offset(offset, includeExternalEdge, includeInternalEdges, tolerance);
            if (face2Ds == null)
                return null;

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }

        public static Triangle3D Offset(this Triangle3D triangle3D, double offset, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = triangle3D?.GetPlane();
            if (plane == null || !plane.IsValid())
            {
                return null;
            }

            Triangle2D triangle2D = Planar.Query.Offset(plane.Convert(triangle3D), offset, tolerance);
            if (triangle2D == null)
            {
                return null;
            }

            return plane.Convert(triangle2D);
        }

        public static List<Polygon3D> Offset(this Polygon3D polygon3D, double offset, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = polygon3D?.GetPlane();
            if (plane == null || !plane.IsValid())
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = Planar.Query.Offset(plane.Convert(polygon3D), offset, tolerance);
            if (polygon2Ds == null)
            {
                return null;
            }

            return polygon2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}