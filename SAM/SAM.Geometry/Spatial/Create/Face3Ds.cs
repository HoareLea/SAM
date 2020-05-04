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
    }
}