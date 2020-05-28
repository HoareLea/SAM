using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static List<Face3D> Face3Ds(this IEnumerable<Planar.IClosed2D> edges, Plane plane, bool orientInternalEdges = true)
        {
            if (plane == null || edges == null || edges.Count() == 0)
                return null;

            List<Planar.Face2D> face2Ds = Planar.Create.Face2Ds(edges, orientInternalEdges);
            if (face2Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            if (face2Ds.Count == 0)
                return result;

            return face2Ds.ConvertAll(x => new Face3D(plane, x));
        }

        public static List<Face3D> Face3Ds(this IEnumerable<Polygon3D> polygon3Ds, bool orientInternalEdges = true, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon3Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();

            List<Polygon3D> polygon3Ds_Temp = new List<Polygon3D>(polygon3Ds);
            polygon3Ds_Temp.RemoveAll(x => x == null);

            if (polygon3Ds_Temp.Count() == 0)
                return result;

            if (polygon3Ds_Temp.Count() == 1)
            {
                result.Add(new Face3D(polygon3Ds_Temp.First()));
                return result;
            }

            polygon3Ds_Temp.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
            while (polygon3Ds_Temp.Count > 0)
            {
                Polygon3D polygon3D = polygon3Ds_Temp.First();
                Plane plane = polygon3D.GetPlane();

                List<Polygon3D> polygon3Ds_Plane = polygon3Ds_Temp.FindAll(x => x.GetPlane().Coplanar(plane, tolerance));
                polygon3Ds_Temp.RemoveAll(x => polygon3Ds_Plane.Contains(x));

                IEnumerable<Planar.IClosed2D> closed2Ds = polygon3Ds_Plane.ConvertAll(x => plane.Convert(plane.Project(x)));
                List<Face3D> face3Ds = Face3Ds(closed2Ds, plane, orientInternalEdges);
                if (face3Ds != null && face3Ds.Count > 0)
                    result.AddRange(face3Ds);
            }

            return result;
        }
    }
}