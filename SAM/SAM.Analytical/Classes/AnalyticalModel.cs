using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class AnalyticalModel : Core.SAMModel
    {
        private Core.Location location;
        private Core.Address address;
        private AdjacencyCluster adjacencyCluster;

        public AnalyticalModel(string name, Core.Location location, Core.Address address, AdjacencyCluster adjacencyCluster)
            : base(name)
        {
            this.location = location;
            this.address = address;
            this.adjacencyCluster = adjacencyCluster;
        }

        public AnalyticalModel(Guid guid, string name)
            : base(guid, name)
        {
        }

        public AnalyticalModel(JObject jObject)
            : base(jObject)
        {
        }

        public Core.Location Location
        {
            get
            {
                return new Core.Location(location);
            }
        }

        public Core.Address Address
        {
            get
            {
                return new Core.Address(address);
            }
        }

        public AdjacencyCluster AdjacencyCluster
        {
            get
            {
                return new AdjacencyCluster(adjacencyCluster);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Location"))
                location = new Core.Location(jObject.Value<JObject>("Location"));

            if (jObject.ContainsKey("Address"))
                address = new Core.Address(jObject.Value<JObject>("Address"));

            if (jObject.ContainsKey("AdjacencyCluster"))
                adjacencyCluster = new AdjacencyCluster(jObject.Value<JObject>("AdjacencyCluster"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (location != null)
                jObject.Add("Location", location.ToJObject());

            if (address != null)
                jObject.Add("Address", address.ToJObject());

            if (adjacencyCluster != null)
                jObject.Add("AdjacencyCluster", adjacencyCluster.ToJObject());

            return jObject;
        }
    }
}
