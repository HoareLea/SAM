using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static BoundaryType BoundaryType(this AnalyticalModel analyticalModel, Panel panel)
        {
            if(panel  == null || analyticalModel == null)
            {
                return Analytical.BoundaryType.Undefined;
            }


            return BoundaryType(analyticalModel.AdjacencyCluster, panel);
        }

        public static BoundaryType BoundaryType(this AdjacencyCluster adjacencyCluster, Panel panel)
        {
            if (adjacencyCluster == null || panel == null)
            {
                return Analytical.BoundaryType.Undefined;
            }

            if(panel.Adiabatic())
            {
                return Analytical.BoundaryType.Adiabatic;
            }
            
            if(adjacencyCluster.Ground(panel))
            {
                return Analytical.BoundaryType.Ground;
            }

            if(adjacencyCluster.ExposedToSun(panel))
            {
                return Analytical.BoundaryType.Exposed;
            }

            List<Space> spaces = adjacencyCluster.GetSpaces(panel);
            if(spaces != null && spaces.Count >= 2)
            {
                return Analytical.BoundaryType.Linked;
            }

            return Analytical.BoundaryType.Undefined;
        }
    }
}