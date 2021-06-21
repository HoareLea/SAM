using Newtonsoft.Json.Linq;

using System.Collections.Generic;

namespace SAM.Architectural
{
    public abstract class HostBuildingElementType : BuildingElementType
    {
        private List<MaterialLayer> materialLayers;

        public HostBuildingElementType(HostBuildingElementType hostBuildingElementType)
            : base(hostBuildingElementType)
        {

        }

        public HostBuildingElementType(JObject jObject)
            : base(jObject)
        {

        }

        public HostBuildingElementType(string name)
            : base(name)
        {

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
