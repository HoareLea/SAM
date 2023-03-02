using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class SpaceZonesFilter : MultiRelationAdjacencyClusterFilter<Zone>
    {

        public SpaceZonesFilter(IFilter filter)
        : base(filter)
        {

        }

        public SpaceZonesFilter(SpaceZonesFilter spaceZonesFilter)
            : base(spaceZonesFilter)
        {

        }

        public SpaceZonesFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override List<Zone> GetRelatives(IJSAMObject jSAMObject)
        {
            Space space = (jSAMObject as Space);
            if(space == null)
            {
                return null;
            }

            return AdjacencyCluster.GetZones(space);
        }
    }
}