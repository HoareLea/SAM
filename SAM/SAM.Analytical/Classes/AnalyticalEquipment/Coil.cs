using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an coil unit object in the analytical domain
    /// </summary>
    public abstract class Coil : SAMObject, IAirHandlingUnitComponent
    {
        private double fluidReturnTemperature;
        private double fluidSupplyTemperature;

        public Coil(string name, double fluidSupplyTemperature, double fluidReturnTemperature)
            : base(name)
        {
            this.fluidReturnTemperature = fluidReturnTemperature;
            this.fluidSupplyTemperature = fluidSupplyTemperature;
        }

        public Coil(JObject jObject)
            : base(jObject)
        {

        }

        public Coil(Coil coil)
            : base(coil)
        {

        }

        public Coil(Guid guid, string name, double fluidSupplyTemperature, double fluidReturnTemperature)
            : base(guid, name)
        {
            this.fluidReturnTemperature = fluidReturnTemperature;
            this.fluidSupplyTemperature = fluidSupplyTemperature;
        }

        public double FluidReturnTemperature
        {
            get
            {
                return fluidReturnTemperature;
            }
        }

        public double FluidSupplyTemperature
        {
            get
            {
                return fluidSupplyTemperature;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("FluidReturnTemperature"))
            {
                fluidReturnTemperature = jObject.Value<double>("FluidReturnTemperature");
            }

            if (jObject.ContainsKey("FluidSupplyTemperature"))
            {
                fluidSupplyTemperature = jObject.Value<double>("FluidSupplyTemperature");
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(!double.IsNaN(fluidSupplyTemperature))
            {
                jObject.Add("FluidSupplyTemperature", fluidSupplyTemperature);
            }

            if (!double.IsNaN(fluidReturnTemperature))
            {
                jObject.Add("FluidReturnTemperature", fluidReturnTemperature);
            }

            return jObject;
        }
    }
}
