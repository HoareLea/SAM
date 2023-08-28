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
        
        private bool summer;
        private double summerOffTemperature;

        private bool winter;
        private double winterOffTemperature;

        public Coil(string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(name)
        {
            this.fluidReturnTemperature = fluidReturnTemperature;
            this.fluidSupplyTemperature = fluidSupplyTemperature;
            this.contactFactor = contactFactor;
            summerOffTemperature = offTemperature;
            winterOffTemperature = offTemperature;
        }

        public Coil(string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double summerOffTemperature, double winterOffTemperature)
            : base(name)
        {
            this.fluidReturnTemperature = fluidReturnTemperature;
            this.fluidSupplyTemperature = fluidSupplyTemperature;
            this.contactFactor = contactFactor;
            this.summerOffTemperature = summerOffTemperature;
            this.winterOffTemperature = winterOffTemperature;
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
                winterOffTemperature = coil.winterOffTemperature;
                summerOffTemperature = coil.summerOffTemperature;
            }
        }

        public Coil(Guid guid, string name, double fluidSupplyTemperature, double fluidReturnTemperature, double contactFactor, double offTemperature)
            : base(guid, name)
        {
            this.fluidReturnTemperature = fluidReturnTemperature;
            this.fluidSupplyTemperature = fluidSupplyTemperature;
            this.contactFactor = contactFactor;
            summerOffTemperature = offTemperature;
            winterOffTemperature = offTemperature;
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

        /// <summary>
        /// Winter temperature of air exiting the coil
        /// </summary>
        public double WinterOffTemperature
        {
            get
            {
                return winterOffTemperature;
            }

            set
            {
                winterOffTemperature = value;
            }
        }

        /// <summary>
        /// Summer temperature of air exiting the coil
        /// </summary>
        public double SummerOffTemperature
        {
            get
            {
                return summerOffTemperature;
            }

            set
            {
                summerOffTemperature = value;
            }
        }

        /// <summary>
        /// Off/On during summer season
        /// </summary>
        public bool Summer
        {
            get
            {
                return summer;
            }

            set
            {
                summer = value;
            }
        }

        /// <summary>
        /// Off/On during winter season
        /// </summary>
        public bool Winter
        {
            get
            {
                return winter;
            }

            set
            {
                winter = value;
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

            if (jObject.ContainsKey("SummerOffTemperature"))
            {
                summerOffTemperature = jObject.Value<double>("SummerOffTemperature");
            }

            if (jObject.ContainsKey("WinterOffTemperature"))
            {
                winterOffTemperature = jObject.Value<double>("WinterOffTemperature");
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

            if (!double.IsNaN(summerOffTemperature))
            {
                jObject.Add("SummerOffTemperature", summerOffTemperature);
            }

            if (!double.IsNaN(winterOffTemperature))
            {
                jObject.Add("WinterOffTemperature", winterOffTemperature);
            }

            return jObject;
        }
    }
}
