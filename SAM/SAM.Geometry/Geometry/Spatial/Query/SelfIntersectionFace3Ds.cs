using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> SelfIntersectionFace3Ds(this Face3D face3D, double maxLength, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face3D == null)
                return null;

            Plane plane = face3D.GetPlane();
            if (plane == null)
                return null;

            Planar.Face2D face2D = plane.Convert(face3D);

            List<Planar.Face2D> face2Ds = Planar.Query.SelfIntersectionFace2Ds(face2D, maxLength, tolerance);
            if (face2Ds == null)
                return null;

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}