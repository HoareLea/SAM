using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class AnalyticalModel : Core.SAMModel
    {
        private string description;
        private Core.Location location;
        private Core.Address address;
        private AdjacencyCluster adjacencyCluster;

        public AnalyticalModel(string name, string description, Core.Location location, Core.Address address, AdjacencyCluster adjacencyCluster)
            : base(name)
        {
            this.description = description;

            if (location != null)
                this.location = new Core.Location(location);

            if (address != null)
                this.address = new Core.Address(address);

            if (adjacencyCluster != null)
                this.adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
        }

        public AnalyticalModel(Guid guid, string name)
            : base(guid, name)
        {
        }

        public AnalyticalModel(JObject jObject)
            : base(jObject)
        {
        }

        public AnalyticalModel(AnalyticalModel analyticalModel)
            : base(analyticalModel)
        {
            if (analyticalModel == null)
                return;

            description = analyticalModel.description;
            
            if (analyticalModel.location != null)
                location = new Core.Location(analyticalModel.location);

            if (analyticalModel.address != null)
                address = new Core.Address(analyticalModel.address);

            if (analyticalModel.adjacencyCluster != null)
                adjacencyCluster = new AdjacencyCluster(analyticalModel.adjacencyCluster);
        }

        public Core.Location Location
        {
            get
            {
                return new Core.Location(location);
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public Core.Address Address
        {
            get
            {
                if (address == null)
                    return null;

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

        public bool AddSpace(Space space, IEnumerable<Panel> panels)
        {
            if (space == null || panels == null)
                return false;

            if (adjacencyCluster == null)
                adjacencyCluster = new AdjacencyCluster();

            return adjacencyCluster.AddSpace(space, panels);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Description"))
                description = jObject.Value<string>("Description");

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

            if (description != null)
                jObject.Add("Description", description);

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
