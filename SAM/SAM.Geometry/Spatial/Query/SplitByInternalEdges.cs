using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> SplitByInternalEdges(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = face3D?.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Face2D face2D = plane.Convert(face3D);
            if(face2D == null)
            {
                return null;
            }

            List<Face2D> face2Ds = face2D.SplitByInternalEdges(tolerance);
            if(face2Ds == null)
            {
                return null;
            }

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}