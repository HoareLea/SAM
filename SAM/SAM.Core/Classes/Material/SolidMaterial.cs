using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public abstract class SolidMaterial : Material
    {
        public SolidMaterial(string name)
            : base(name)
        {

        }

        public SolidMaterial(Guid guid, string name, string displayName, string description, double thermalConductivity, double density, double specificHeatCapacity)
        : base(guid, name, displayName, description, thermalConductivity, density, specificHeatCapacity)
        {

        }

        public SolidMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeatCapacity, double density)
            : base(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density)
        {

        }

        public SolidMaterial(string name, Guid guid, SolidMaterial solidMaterial, string displayName, string description)
            : base(name, guid, solidMaterial, displayName, description)
        {

        }

        public SolidMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public SolidMaterial(JObject jObject)
            : base(jObject)
        {

        }

        public SolidMaterial(SolidMaterial solidMaterial)
            : base(solidMaterial)
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