using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Linq;
using SAM.Architectural;

namespace SAM.Analytical
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

        public HostPartitionType(System.Guid guid, string name)
            : base(guid, name)
        {

        }

        public HostPartitionType(string name, IEnumerable<MaterialLayer> materialLayers)
            : base(name)
        {
            this.materialLayers = materialLayers?.ToList().ConvertAll(x => new MaterialLayer(x));
        }

        public HostPartitionType(System.Guid guid, string name, IEnumerable<MaterialLayer> materialLayers)
            : base(guid, name)
        {
            this.materialLayers = materialLayers?.ToList().ConvertAll(x => new MaterialLayer(x));
        }

        public HostPartitionType(HostPartitionType hostPartitionType, string name)
            : base(hostPartitionType, name)
        {
            materialLayers = hostPartitionType?.materialLayers?.ToList().ConvertAll(x => new MaterialLayer(x));
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

            set
            {
                if(value == null)
                {
                    return;
                }

                materialLayers = value?.ConvertAll(x => new MaterialLayer(x));
            }
        }

        public MaterialLayer this[int i]
        {
            get
            {
                if(materialLayers == null)
                {
                    return null;
                }

                return new MaterialLayer(materialLayers[i]);
            }

            set
            {
                if (materialLayers == null)
                {
                    return;
                }

                materialLayers[i] = new MaterialLayer(value);
            }
        }

        public double GetThickness()
        {
            if(materialLayers == null)
            {
                return double.NaN;
            }

            double result = 0;
            foreach(MaterialLayer materialLayer in materialLayers)
            {
                double thickness = materialLayer.Thickness;
                if(double.IsNaN(thickness))
                {
                    continue;
                }
                
                result += thickness;
            }

            return result;
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
