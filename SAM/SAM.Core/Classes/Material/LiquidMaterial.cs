using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public class LiquidMaterial : FluidMaterial
    {
        public LiquidMaterial(string name)
            : base(name)
        {

        }

        public LiquidMaterial(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeatCapacity, double dynamicViscosity)
        : base(guid, name, displayName, description, thermalConductivity, density, specificHeatCapacity, dynamicViscosity)
        {

        }

        public LiquidMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeatCapacity, double density, double dynamicViscosity)
            : base(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density, dynamicViscosity)
        {

        }
        
        public LiquidMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public LiquidMaterial(JObject jObject)
            : base(jObject)
        {
        }

        public LiquidMaterial(LiquidMaterial liquidMaterial)
            : base(liquidMaterial)
        {

        }

        public LiquidMaterial(string name, Guid guid, LiquidMaterial liquidMaterial, string displayName, string description)
            :base(name, guid, liquidMaterial, displayName, description)
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