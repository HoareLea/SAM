using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an heating coil unit object in the analytical domain
    /// </summary>
    public class HeatingCoil : Coil
    {
        public HeatingCoil(string name, double fluidSupplyTemperature, double fluidReturnTemperature)
            : base(name, fluidSupplyTemperature, fluidReturnTemperature)
        {

        }

        public HeatingCoil(JObject jObject)
            : base(jObject)
        {

        }

        public HeatingCoil(HeatingCoil heatingCoil)
            : base(heatingCoil)
        {

        }

        public HeatingCoil(Guid guid, string name, double fluidSupplyTemperature, double fluidReturnTemperature)
            : base(guid, name, fluidSupplyTemperature, fluidReturnTemperature)
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
