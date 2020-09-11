using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public class GasMaterial : Material
    {
        public GasMaterial(string name)
            : base(name)
        {

        }

        public GasMaterial(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeat)
        : base(guid, name, displayName, description, thermalConductivity, density, specificHeat)
        {

        }

        public GasMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeat, double density)
            : base(name, group, displayName, description, thermalConductivity, specificHeat, density)
        {

        }

        public GasMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public GasMaterial(JObject jObject)
            : base(jObject)
        {
        }

        public GasMaterial(GasMaterial gasMaterial)
            : base(gasMaterial)
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