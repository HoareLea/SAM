using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Face2D> Face2Ds(this IEnumerable<IClosed2D> edges, bool orientInternalEdges = true)
        {
            if (edges == null)
                return null;

            List<Face2D> result = new List<Face2D>();
            if (edges.Count() == 0)
                return result;

            List<Planar.IClosed2D> edges_Current = new List<IClosed2D>(edges);
            while (edges_Current.Count > 0)
            {
                List<IClosed2D> edges_Excluded = null;
                Face2D face = Face2D.Create(edges_Current, out edges_Excluded, orientInternalEdges);
                if (face == null)
                    break;

                result.Add(face);

                edges_Current = edges_Excluded;
            }

            return result;
        }
    }
}