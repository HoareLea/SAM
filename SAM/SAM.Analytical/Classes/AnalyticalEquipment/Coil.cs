using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an coil unit object in the analytical domain
    /// </summary>
    public abstract class Coil : SimpleEquipment
    {
        private double fluidReturnTemperature;
        private double fluidSupplyTemperature;
        private double contactFactor;
        private double offTemperature;

        public Coil(string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(name)
        {
            this.fluidReturnTemperature = fluidReturnTemperature;
            this.fluidSupplyTemperature = fluidSupplyTemperature;
            this.contactFactor = contactFactor;
            this.offTemperature = offTemperature;
        }

        public Coil(JObject jObject)
            : base(jObject)
        {

        }

        public Coil(Coil coil)
            : base(coil)
        {
            if(coil != null)
            {
                fluidReturnTemperature = coil.fluidReturnTemperature;
                fluidSupplyTemperature = coil.fluidSupplyTemperature;
                contactFactor = coil.contactFactor;
                offTemperature = coil.offTemperature;
            }
        }

        public Coil(Guid guid, string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(guid, name)
        {
            this.fluidReturnTemperature = fluidReturnTemperature;
            this.fluidSupplyTemperature = fluidSupplyTemperature;
            this.contactFactor = contactFactor;
            this.offTemperature = offTemperature;
        }

        public double FluidReturnTemperature
        {
            get
            {
                return fluidReturnTemperature;
            }

            set
            {
                fluidReturnTemperature = value;
            }
        }

        public double FluidSupplyTemperature
        {
            get
            {
                return fluidSupplyTemperature;
            }

            set
            {
                fluidReturnTemperature = value;
            }
        }

        public double ContactFactor
        {
            get
            {
                return contactFactor;
            }

            set
            {
                contactFactor = value;
            }
        }

        public double OffTemperature
        {
            get
            {
                return offTemperature;
            }

            set
            {
                offTemperature = value;
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

            if (jObject.ContainsKey("ContactFactor"))
            {
                contactFactor = jObject.Value<double>("ContactFactor");
            }

            if (jObject.ContainsKey("OffTemperature"))
            {
                offTemperature = jObject.Value<double>("OffTemperature");
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

            if (!double.IsNaN(contactFactor))
            {
                jObject.Add("ContactFactor", contactFactor);
            }

            if (!double.IsNaN(offTemperature))
            {
                jObject.Add("OffTemperature", offTemperature);
            }

            return jObject;
        }
    }
}
