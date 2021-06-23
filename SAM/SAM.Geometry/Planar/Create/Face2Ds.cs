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

            List<Face2D> face2Ds = new List<Face2D>();
            if (edges.Count() == 0)
                return face2Ds;

            List<IClosed2D> edges_Current = new List<IClosed2D>(edges);
            while (edges_Current.Count > 0)
            {
                List<IClosed2D> edges_Excluded = null;
                Face2D face2D = Create.Face2D(edges_Current, out edges_Excluded, orientInternalEdges);
                if (face2D == null)
                    break;

                if (face2D.GetInternalPoint2D() != null)
                    face2Ds.Add(face2D);

                edges_Current = edges_Excluded;
            }

            if (face2Ds.Count == 1)
                return face2Ds;

            face2Ds.Sort((x, y) => x.ExternalEdge2D.GetArea().CompareTo(y.ExternalEdge2D.GetArea()));
            List<Face2D> result = new List<Face2D>();
            while(face2Ds.Count > 0)
            {
                Face2D face2D = face2Ds[0];
                face2Ds.RemoveAt(0);

                List<Face2D> faces2D_Inside = face2Ds.FindAll(x => face2D.Inside(x.InternalPoint2D()));
                if (faces2D_Inside.Count != 0)
                    continue;

                result.Add(face2D);
            }

            return result;
        }
    }
}