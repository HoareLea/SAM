using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool AddSpace(this AdjacencyCluster adjacencyCluster, Space space, IEnumerable<Panel> panels)
        {
            if (adjacencyCluster == null || space == null || panels == null)
                return false;

            if (!adjacencyCluster.AddObject(space))
                return false;

            foreach(Panel panel in panels)
            {
                if (adjacencyCluster.AddObject(panel))
                    adjacencyCluster.AddRelation(space, panel);
            }

            return true;
        }
    }
}