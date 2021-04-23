using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Face2D Face2D(this IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges, bool orientInternalEdges = true)
        {
            return Planar.Face2D.Create(externalEdge, internalEdges, orientInternalEdges);
        }

        public static Face2D Face2D(IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded, bool orientInternalEdges = true)
        {
            return Planar.Face2D.Create(edges, out edges_Excluded, orientInternalEdges);
        }

        public static Face2D Face2D(IEnumerable<IClosed2D> edges, bool orientInternalEdges = true)
        {
            return Planar.Face2D.Create(edges, orientInternalEdges);
        }
    }
}