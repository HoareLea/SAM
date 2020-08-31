using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool UpdateSpace(this AdjacencyCluster adjacencyCluster, Space space, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || space == null)
                return false;

            Geometry.Spatial.Point3D point3D = space.Location;
            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return false;

            foreach(Space space_Temp in spaces)
            {
                bool inside = adjacencyCluster.Inside(space_Temp, point3D, silverSpacing, tolerance);
                if(inside)
                {
                    List<Panel> panels = adjacencyCluster.GetPanels(space_Temp);
                    adjacencyCluster.RemoveObject(typeof(Space), space_Temp.Guid);

                    adjacencyCluster.AddObject(space);
                    if(panels != null && panels.Count != 0)
                    {
                        foreach (Panel panel in panels)
                            adjacencyCluster.AddRelation(space, panel);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}