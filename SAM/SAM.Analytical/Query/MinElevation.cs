using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double MinElevation(this Panel panel)
        {
            return MinElevation(panel?.PlanarBoundary3D);
        }

        public static double MinElevation(this PlanarBoundary3D planarBoundary3D)
        {
            Geometry.Spatial.BoundingBox3D boundingBox3D = planarBoundary3D?.GetBoundingBox();
            if (boundingBox3D == null)
                return double.NaN;

            return boundingBox3D.Min.Z;
        }

        public static double MinElevation(this IEnumerable<Panel> panels)
        {
            if (panels == null || panels.Count() == 0)
                return double.NaN;

            double result = double.MaxValue;
            foreach(Panel panel in panels)
            {
                double minElevation = panel.MinElevation();
                if (double.IsNaN(minElevation))
                    continue;

                if (minElevation < result)
                    result = minElevation;
            }

            return result;
        }

        public static double MinElevation(this Space space, AdjacencyCluster adjacencyCluster)
        {
            List<Panel> panels = adjacencyCluster?.GetPanels(space);
            if (panels == null || panels.Count == 0)
                return double.NaN;

            return MinElevation(panels);
        }
    }
}