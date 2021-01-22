using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static GuidCollection UpdateGroup(this AdjacencyCluster adjacencyCluster, string name, AnalyticalZoneType analyticalZoneType, params Space[] spaces)
        {
            if (adjacencyCluster == null || name == null)
                return null;

            GuidCollection result = null;

            List<GuidCollection> groups = adjacencyCluster.GetGroups(name);
            result = groups?.Find(x => x.TryGetValue(AnalyticalZoneParameter.AnalyticalZoneType, out string analyticalZoneType_Text) && analyticalZoneType.Text().Equals(analyticalZoneType_Text));
            if(result == null)
            {
                System.Guid guid;
                do
                    guid = System.Guid.NewGuid();
                while (adjacencyCluster.GetGroup(result.Guid) != null);

                result = Create.GuidCollection(guid, name, analyticalZoneType);
                if (result == null)
                    return null;
            }

            if (spaces != null)
                foreach (Space space in spaces)
                    if (space != null)
                        result.Add(space.Guid);

            result = adjacencyCluster.UpdateGroup(result);
            return result;
        }
    }
}