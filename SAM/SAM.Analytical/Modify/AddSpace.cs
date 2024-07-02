using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool AddSpace(this AdjacencyCluster adjacencyCluster, Space space, IEnumerable<IPanel> panels = null)
        {
            if (adjacencyCluster == null || space == null)
            {
                return false;
            }

            if (!adjacencyCluster.AddObject(space))
            {
                return false;
            }
            
            if(panels != null)
            {
                foreach (Panel panel in panels)
                {
                    if (adjacencyCluster.AddObject(panel))
                    {
                        adjacencyCluster.AddRelation(space, panel);
                    }
                }
            }

            return true;
        }
    }
}