using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Zone Zone(this AdjacencyCluster adjacencyCluster, string name, ZoneType zoneType)
        {
            if (adjacencyCluster == null || string.IsNullOrWhiteSpace(name))
                return null;

            List<Zone> zones = adjacencyCluster.GetGroups<Zone>(name);
            if (zones == null || zones.Count == 0)
                return null;

            string zoneTypeText = zoneType.Text();

            foreach(Zone zone in zones)
            {
                if (!zone.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory))
                    continue;

                if (zoneTypeText.Equals(zoneCategory))
                    return zone;
            }

            return null;
        }
    }
}