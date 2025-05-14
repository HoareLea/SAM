using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

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

                Panel panel_New = PanelsByFace3D(panels, face3D, areaFactor, maxDistance, out List<double> intersectionAreas, tolerance_Angle, tolerance_Distance)?.FirstOrDefault();
                if(panel_New == null)
                {
                    if(panel.PanelGroup == Analytical.PanelGroup.Floor && spaces != null && spaces.Count > 1)
                    {
                        panel_Updated = new Panel(panel, null);
                        panel_Updated = new Panel(panel_Updated, Analytical.PanelType.Air);
                        if(panel_Updated.Normal.SameHalf(Vector3D.WorldZ))
                        {
                            panel_Updated.FlipNormal(true, false);
                        }

                        panel_Updated.RemoveApertures();

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
                    if(intersectionAreas[0]/area < areaFactor)
                    {
                        panel_Updated = new Panel(panel, null);
    
                        if (panel_Updated.Normal.SameHalf(Vector3D.WorldZ))
                        {
                            panel_Updated.FlipNormal(true, false);
                        }

                        panel_Updated.RemoveApertures();
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