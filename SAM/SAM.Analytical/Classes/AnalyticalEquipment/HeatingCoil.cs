using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an heating coil object in the analytical domain
    /// </summary>
    public class HeatingCoil : Coil
    {
        public HeatingCoil(double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base("Heating Coil", fluidSupplyTemperature, fluidReturnTemperature, contactFactor, offTemperature)
        {

        }

        public HeatingCoil(string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(name, fluidSupplyTemperature, fluidReturnTemperature, contactFactor, offTemperature)
        {
 
        }

        public HeatingCoil(string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double summerOffTemperature, double winterOffTemperature)
            : base(name, fluidSupplyTemperature, fluidReturnTemperature, contactFactor, summerOffTemperature, winterOffTemperature)
        {

        }

        public HeatingCoil(double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double summerOffTemperature, double winterOffTemperature)
            : base("Heating Coil", fluidSupplyTemperature, fluidReturnTemperature, contactFactor, summerOffTemperature, winterOffTemperature)
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

        public HeatingCoil(Guid guid, string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(guid, name, fluidSupplyTemperature, fluidReturnTemperature, contactFactor, offTemperature)
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
                return null;

            return jObject;
        }
    }
}
