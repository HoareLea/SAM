using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Zone Zone(this AdjacencyCluster adjacencyCluster, string name, ZoneType zoneType)
        {
            if (adjacencyCluster == null || string.IsNullOrWhiteSpace(name))
                return null;

            List<Zone> zones = adjacencyCluster.GetObjects<Zone>()?.FindAll(x => name.Equals(x.Name));
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