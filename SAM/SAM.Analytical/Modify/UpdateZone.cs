using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Zone UpdateZone(this AdjacencyCluster adjacencyCluster, string name, ZoneType zoneType, params Space[] spaces)
        {
            if (adjacencyCluster == null || name == null)
                return null;

            return UpdateZone(adjacencyCluster, name, zoneType.Text(), spaces);
        }

        public static Zone UpdateZone(this AdjacencyCluster adjacencyCluster, string name, string zoneCategory, params Space[] spaces)
        {
            if (adjacencyCluster == null || name == null)
                return null;

            Zone result = null;

            List<Zone> zones = adjacencyCluster.GetObjects<Zone>()?.FindAll(x => x.Name == name);

            if (zoneCategory != null)
                result = zones?.Find(x => x.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory_Temp) && zoneCategory.Equals(zoneCategory_Temp));

            if (result == null)
            {
                System.Guid guid;
                do
                {
                    guid = System.Guid.NewGuid();
                }
                while (adjacencyCluster.GetObject<Zone>(guid) != null);

                result = Create.Zone(guid, name, zoneCategory);
                if (result == null)
                    return null;

                adjacencyCluster.AddObject(result);
            }

            if (spaces != null)
                foreach (Space space in spaces)
                    if (space != null)
                        adjacencyCluster.AddRelation(result, space);

            return Core.Query.Clone(result);
        }
    }
}