using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Geometry.Spatial.Shell Shell(this AdjacencyCluster adjacencyCluster, Space space)
        {
            if (adjacencyCluster == null || space == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetRelatedObjects<Panel>(space);
            if (panels == null || panels.Count == 0)
                return null;

            return new Geometry.Spatial.Shell(panels.ConvertAll(x => x.GetFace3D()));
        }
    }
}