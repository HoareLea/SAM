using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural
{
    public abstract class HostPartitionType : BuildingElementType
    {
        private List<MaterialLayer> materialLayers;

        public HostPartitionType(HostPartitionType hostPartitionType)
            : base(hostPartitionType)
        {

        }

        public HostPartitionType(JObject jObject)
            : base(jObject)
        {

        }

        public HostPartitionType(string name)
            : base(name)
        {

        }

        public HostPartitionType(string name, IEnumerable<MaterialLayer> materialLayers)
            : base(name)
        {
            this.materialLayers = materialLayers?.ToList().ConvertAll(x => new MaterialLayer(x));
        }

        public List<MaterialLayer> MaterialLayers
        {
            get
            {
                if (materialLayers == null)
                {
                    return null;
                }

                return materialLayers.ConvertAll(x => new MaterialLayer(x));
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("MaterialLayers"))
                materialLayers = Core.Create.IJSAMObjects<MaterialLayer>(jObject.Value<JArray>("MaterialLayers"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
                return jObject;

            if (materialLayers != null)
                jObject.Add("MaterialLayers", Core.Create.JArray(materialLayers));

            return jObject;
        }

    }
}
