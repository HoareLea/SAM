using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class AdjacencyClusterSimulationResult : Result
    {
        public AdjacencyClusterSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public AdjacencyClusterSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public AdjacencyClusterSimulationResult(AdjacencyClusterSimulationResult adjacencyClusterSimulationResult)
            : base(adjacencyClusterSimulationResult)
        {

        }

        public AdjacencyClusterSimulationResult(Guid guid, AdjacencyClusterSimulationResult adjacencyClusterSimulationResult)
            : base(guid, adjacencyClusterSimulationResult)
        {

        }

        public AdjacencyClusterSimulationResult(JObject jObject)
            : base(jObject)
        {
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            return jObject;
        }
    }
}