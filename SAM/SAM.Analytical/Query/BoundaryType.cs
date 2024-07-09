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

        public static BoundaryType BoundaryType(this AdjacencyCluster adjacencyCluster, IPanel panel)
        {
            if (adjacencyCluster == null || panel == null)
            {
                return Analytical.BoundaryType.Undefined;
            }

            if (panel is ExternalPanel)
            {
                return Analytical.BoundaryType.Exposed;
            }

            Panel panel_Temp = panel as Panel;
            if(panel_Temp == null)
            {
                return Analytical.BoundaryType.Undefined;
            }

            if(panel_Temp.Adiabatic())
            {
                return Analytical.BoundaryType.Adiabatic;
            }

            if(adjacencyCluster.Shade(panel_Temp))
            {
                return Analytical.BoundaryType.Shade;
            }
            
            if(adjacencyCluster.Ground(panel_Temp))
            {
                return Analytical.BoundaryType.Ground;
            }

            if(adjacencyCluster.ExposedToSun(panel_Temp))
            {
                return Analytical.BoundaryType.Exposed;
            }

            List<Space> spaces = adjacencyCluster.GetSpaces(panel_Temp);
            if(spaces != null && spaces.Count >= 2)
            {
                return Analytical.BoundaryType.Linked;
            }

            if(panel_Temp.PanelType == Analytical.PanelType.Shade || panel_Temp.PanelType == Analytical.PanelType.SolarPanel)
            {
                return Analytical.BoundaryType.Shade;
            }

            return Analytical.BoundaryType.Undefined;
        }
    }
}