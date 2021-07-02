using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Face2D Face2D(this IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges, EdgeOrientationMethod edgeOrientationMethod= EdgeOrientationMethod.Opposite)
        {
            return Planar.Face2D.Create(externalEdge, internalEdges, edgeOrientationMethod);
        }

        public static Face2D Face2D(IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            return Planar.Face2D.Create(edges, out edges_Excluded, edgeOrientationMethod);
        }

        public static Face2D Face2D(IEnumerable<IClosed2D> edges, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            return Planar.Face2D.Create(edges, edgeOrientationMethod);
        }
    }
}