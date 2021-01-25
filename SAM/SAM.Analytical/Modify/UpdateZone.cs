using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Zone UpdateZone(this AdjacencyCluster adjacencyCluster, string name, ZoneType zoneType, params Space[] spaces)
        {
            if (adjacencyCluster == null || name == null)
                return null;

            Zone result = null;

            List<Zone> zones = adjacencyCluster.GetGroups<Zone>(name);
            result = zones?.Find(x => x.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory) && zoneType.Text().Equals(zoneCategory));
            if(result == null)
            {
                System.Guid guid;
                do
                    guid = System.Guid.NewGuid();
                while (adjacencyCluster.GetGroup(guid) != null);

                result = Create.Zone(guid, name, zoneType);
                if (result == null)
                    return null;
            }

            if (spaces != null)
                foreach (Space space in spaces)
                    if (space != null)
                        result.Add(space.Guid);

            result = adjacencyCluster.UpdateGroup(result) as Zone;
            return result;
        }
    }
}