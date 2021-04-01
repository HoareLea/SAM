using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Triangle3D> Triangulate(this Face3D face3D, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            Planar.Face2D face2D = plane.Convert(face3D);
            if (face2D == null)
                return null;

            List<Planar.Triangle2D> triangle2Ds = Planar.Query.Triangulate(face2D, tolerance);
            if (triangle2Ds == null)
                return null;

            return triangle2Ds.ConvertAll(x => plane.Convert(x));

        }

        public static List<Triangle3D> Triangulate(this Polygon3D polygon3D, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = polygon3D?.GetPlane();
            if (plane == null)
                return null;

            Planar.Polygon2D polygon2D = plane.Convert(polygon3D);
            if (polygon2D == null)
                return null;

            List<Planar.Triangle2D> triangle2Ds = Planar.Query.Triangulate(polygon2D, tolerance);
            if (triangle2Ds == null)
                return null;

            return triangle2Ds.ConvertAll(x => plane.Convert(x));
        }

        public static List<Triangle3D> Triangulate(this Shell shell, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Face3D> face3Ds = shell?.Face3Ds;
            if (face3Ds == null)
                return null;

            List<Triangle3D> result = new List<Triangle3D>();
            foreach(Face3D face3D in face3Ds)
            {
                List<Triangle3D> triangle3Ds = face3D?.Triangulate(tolerance);
                if (triangle3Ds == null || triangle3Ds.Count == 0)
                    continue;

                result.AddRange(triangle3Ds);
            }

            return result;

        }
    }
}