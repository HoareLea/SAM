using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public class TransparentMaterial : SolidMaterial
    {
        public TransparentMaterial(string name)
            : base(name)
        {

        }

        public TransparentMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeatCapacity, double density)
            : base(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density)
        {

        }

        public TransparentMaterial(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeatCapacity)
            : base(guid, name, displayName, description, thermalConductivity, density, specificHeatCapacity)
        {

        }

        public TransparentMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public TransparentMaterial(JObject jObject)
            : base(jObject)
        {
            
        }

        public TransparentMaterial(TransparentMaterial transparentMaterial)
            : base(transparentMaterial)
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
                return jObject;

            return jObject;
        }
    }
}