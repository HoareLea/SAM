using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Snap(this Face3D face3D_1, Face3D face3D_2, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane_1 = face3D_1?.GetPlane();
            if (plane_1 == null)
                return null;

            Plane plane_2 = face3D_2?.GetPlane();
            if (plane_2 == null)
                return null;

            if (plane_1.Coplanar(plane_2, tolerance))
            {
                Planar.Face2D face2D_1 = plane_1.Convert(face3D_1);
                Planar.Face2D face2D_2 = plane_1.Convert(plane_1.Project(face3D_2));

                List<Planar.Face2D> face2Ds = Planar.Query.Snap(face2D_1, face2D_2, snapDistance, tolerance);
                if (face2Ds == null)
                    return null;

                return face2Ds?.ConvertAll(x => plane_1.Convert(x));
            }

            PlanarIntersectionResult planarIntersectionResult = plane_1.Intersection(plane_2);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            List<Segment3D> segment3Ds = planarIntersectionResult.GetGeometry3Ds<Segment3D>();
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            throw new NotImplementedException();
        }
    }
}