using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static GuidCollection Group(this AdjacencyCluster adjacencyCluster, string name, AnalyticalZoneType analyticalZoneType)
        {
            if (adjacencyCluster == null || string.IsNullOrWhiteSpace(name))
                return null;

            List<GuidCollection> groups = adjacencyCluster.GetGroups(name);
            if (groups == null || groups.Count == 0)
                return null;

            foreach(GuidCollection group in groups)
            {
                if (!group.TryGetValue(AnalyticalZoneParameter.AnalyticalZoneType, out AnalyticalZoneType analyticalZoneType_Temp))
                    continue;

                if (analyticalZoneType_Temp == analyticalZoneType)
                    return group;
            }

            return null;
        }
    }
}