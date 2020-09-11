using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public class OpaqueMaterial : SolidMaterial
    {
        public OpaqueMaterial(string name)
            : base(name)
        {

        }

        public OpaqueMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeat, double density)
            : base(name, group, displayName, description, thermalConductivity, specificHeat, density)
        {

        }

        public OpaqueMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public OpaqueMaterial(JObject jObject)
            : base(jObject)
        {
        }

        public OpaqueMaterial(OpaqueMaterial opaqueMaterial)
            : base(opaqueMaterial)
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