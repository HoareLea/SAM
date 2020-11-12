using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Cut(this IEnumerable<Face3D> face3Ds, double elevation, double tolerance = Core.Tolerance.Distance)
        {
            if (face3Ds == null || double.IsNaN(elevation))
                return null;

            Plane plane = Plane.WorldXY;
            plane.GetMoved(new Vector3D(0, 0, elevation));

            List<Face3D> result = new List<Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                if(!(boundingBox3D.Max.Z > elevation && boundingBox3D.Min.Z < elevation))
                {
                    result.Add(face3D);
                    continue;
                }

                List<Face3D> face3Ds_Cut = face3D.Cut(plane, tolerance);
                if(face3Ds_Cut == null || face3Ds_Cut.Count == 0 || face3Ds_Cut.Count == 1)
                {
                    result.Add(face3D);
                    continue;
                }

                result.AddRange(face3Ds_Cut);
            }

            return result;
        }

        public static List<Face3D> Cut(this Face3D face3D, Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            PlanarIntersectionResult planarIntersectionResult = plane?.Intersection(face3D, tolerance);
            if (planarIntersectionResult == null)
                return null;

            if (!planarIntersectionResult.Intersecting)
                return null;

            List<Segment3D> segment3Ds = planarIntersectionResult.GetGeometry3Ds<Segment3D>();
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            Plane plane_Face3D = face3D.GetPlane();
            Face2D face2D = plane_Face3D.Convert(face3D);
            if (face2D == null)
                return null;

            List<Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane_Face3D.Convert(plane_Face3D.Project(x)));
            if (segment2Ds == null)
                return null;

            List<Face2D> face2Ds = face2D.Cut(segment2Ds, tolerance);
            if (face2Ds == null || face2Ds.Count == 0)
                return null;

            return face2Ds.ConvertAll(x => plane_Face3D.Convert(x));
        }
    }
}