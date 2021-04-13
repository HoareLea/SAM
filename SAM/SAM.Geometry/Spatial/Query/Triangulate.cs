using System.Collections.Generic;
using System.Linq;

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

            List<List<Triangle3D>> triangle3Ds = Enumerable.Repeat<List<Triangle3D>>(null, face3Ds.Count).ToList();
            System.Threading.Tasks.Parallel.For(0, face3Ds.Count, (int i) =>
            {
                triangle3Ds[i] = face3Ds[i]?.Triangulate(tolerance);
            });

            List<Triangle3D> result = new List<Triangle3D>();
            foreach(List<Triangle3D> triangle3Ds_Temp in triangle3Ds)
            {
                if(triangle3Ds_Temp != null && triangle3Ds_Temp.Count > 0)
                {
                    result.AddRange(triangle3Ds_Temp);
                }
            }

            return result;
        }
    }
}