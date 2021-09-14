using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AnalyticalModel UpdateConstructionsByPanels(this AnalyticalModel analyticalModel, IEnumerable<Panel> panels, double areaFactor, double maxDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(analyticalModel == null)
            {
                return null;
            }

            AdjacencyCluster adjacencyCluster = UpdateConstructionsByPanels(analyticalModel.AdjacencyCluster, panels, areaFactor, maxDistance, tolerance_Angle, tolerance_Distance);
            if(adjacencyCluster == null)
            {
                return new AnalyticalModel(analyticalModel);
            }    

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        public static AdjacencyCluster UpdateConstructionsByPanels(this AdjacencyCluster adjacencyCluster, IEnumerable<Panel> panels, double areaFactor, double maxDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(adjacencyCluster == null)
            {
                return null;
            }

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            List<Panel> panels_AdjacencyCluster = adjacencyCluster.GetPanels();
            if(panels_AdjacencyCluster == null || panels_AdjacencyCluster.Count == 0)
            {
                return result;
            }

            foreach(Panel panel in panels_AdjacencyCluster)
            {
                Face3D face3D = panel?.GetFace3D();
                if(face3D == null)
                {
                    continue;
                }

                Panel panel_Updated = null;

                List<Space> spaces = result.GetSpaces(panel);

                Panel panel_New = PanelByFace3D(panels, face3D, areaFactor, maxDistance, out double intersectionArea, tolerance_Angle, tolerance_Distance);
                if(panel_New == null)
                {
                    if(panel.PanelGroup == Analytical.PanelGroup.Floor && spaces != null && spaces.Count > 1)
                    {
                        panel_Updated = new Panel(panel, null);
                        panel_Updated = new Panel(panel_Updated, Analytical.PanelType.Air);
                        result.AddObject(panel_Updated);
                    }
                    
                    continue;
                }

                if (spaces == null || spaces.Count < 2)
                {
                    panel_Updated = new Panel(panel, panel_New.Construction);
                }
                else
                {
                    double area = face3D.GetArea();
                    if(intersectionArea/area < areaFactor)
                    {
                        panel_Updated = new Panel(panel, null);
                        panel_Updated = new Panel(panel_Updated, Analytical.PanelType.Air);
                    }
                    else
                    {
                        panel_Updated = new Panel(panel, panel_New.Construction);
                    }
                }

                result.AddObject(panel_Updated);
            }

            return result;
        }
    }
}