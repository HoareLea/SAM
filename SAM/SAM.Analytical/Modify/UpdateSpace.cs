using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool UpdateSpace(this AdjacencyCluster adjacencyCluster, Space space, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || space == null)
                return false;

            Point3D point3D = space.Location;
            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return false;

            foreach(Space space_Temp in spaces)
            {
                Shell shell = adjacencyCluster.Shell(space_Temp);
                if (shell == null)
                    continue;

                bool inRange = shell.InRange(point3D, tolerance);
                if(inRange)
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