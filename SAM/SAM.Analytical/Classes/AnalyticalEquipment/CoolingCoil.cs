using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an cooling coil object in the analytical domain
    /// </summary>
    public class CoolingCoil : Coil
    {
        public CoolingCoil(string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(name, fluidSupplyTemperature, fluidReturnTemperature, contactFactor, offTemperature)
        {

        }

        public CoolingCoil(JObject jObject)
            : base(jObject)
        {

        }

        public CoolingCoil(CoolingCoil coolingCoil)
            : base(coolingCoil)
        {

        }

        public CoolingCoil(Guid guid, string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(guid, name, fluidSupplyTemperature, fluidReturnTemperature, contactFactor, offTemperature)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            // TODO: Implement specific deserialization logic for AirHandlingUnit properties

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            // TODO: Implement specific serialization logic for AirHandlingUnit properties

            return jObject;
        }
    }
}
