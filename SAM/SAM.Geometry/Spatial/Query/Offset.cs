using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Offset(this Face3D face3D, double offset, bool includeExternalEdge = true, bool includeInternalEdges = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            List<Face2D> face2Ds = plane.Convert(face3D)?.Offset(offset, includeExternalEdge, includeInternalEdges, tolerance);
            if (face2Ds == null)
                return null;

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}