using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Space> AdjacentSpaces(this AdjacencyCluster adjacencyCluster, Space space)
        {
            if(adjacencyCluster == null || space == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels(space);
            if(panels == null)
            {
                return null;
            }

            List<Space> result = new List<Space>();
            foreach(Panel panel in panels)
            {
                List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                if(spaces == null)
                {
                    continue;
                }

                foreach(Space space_Temp in spaces)
                {
                    if(space_Temp == null || space_Temp.Guid == space.Guid)
                    {
                        continue;
                    }

                    if(result.Find(x => x.Guid == space_Temp.Guid) != null)
                    {
                        continue;
                    }

                    result.Add(space_Temp);
                }
            }

            return result;
        }
    }
}