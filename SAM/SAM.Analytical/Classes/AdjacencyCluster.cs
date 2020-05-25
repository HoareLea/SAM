using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class AdjacencyCluster : Core.RelationCluster
    {        
        public AdjacencyCluster(AdjacencyCluster adjacencyCluster)
            :base(adjacencyCluster)
        {

        }
        
        public AdjacencyCluster(JObject jObject)
            : base(jObject)
        {

        }
        
        public override bool IsValid(Type type)
        {
            if (!base.IsValid(type))
                return false;

            return typeof(Panel).IsAssignableFrom(type) || typeof(Space).IsAssignableFrom(type);
        }

        public bool Internal(Panel panel)
        {
            if (panel == null)
                return false;

            if (!Contains(typeof(Panel)))
                return false;

            List<Space> spaces = GetRelatedObjects<Space>(panel);
            return spaces != null && spaces.Count > 1;
        }
    }
}
