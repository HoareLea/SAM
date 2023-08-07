using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an air handling unit object in the analytical domain
    /// </summary>
    public class AirHandlingUnit : ComplexEquipment
    {
        private double summerSupplyTemperature;
        private double winterSupplyTemperature;

        /// <summary>
        /// Constructor that takes a name parameter and sets the base object's name
        /// </summary>
        /// <param name="name">The name of the air handling unit</param>
        /// <param name="summerSupplyTemperature">Summer Supply Temperature [°C]</param>
        /// <param name="winterSupplyTemperature">Winter Supply Temperature [°C]</param>
        public AirHandlingUnit(string name, double summerSupplyTemperature, double winterSupplyTemperature)
            : base(name)
        {
            this.winterSupplyTemperature = winterSupplyTemperature;
            this.summerSupplyTemperature = summerSupplyTemperature;
        }

        /// <summary>
        /// Constructor that takes a JObject parameter and creates a new object from it
        /// </summary>
        /// <param name="jObject">The JObject containing the air handling unit data</param>
        public AirHandlingUnit(JObject jObject)
            : base(jObject)
        {

        }

        /// <summary>
        /// Constructor that takes another AirHandlingUnit object and creates a new object from it
        /// </summary>
        /// <param name="airHandlingUnit">The AirHandlingUnit object to copy</param>
        public AirHandlingUnit(AirHandlingUnit airHandlingUnit)
            : base(airHandlingUnit)
        {
            if(airHandlingUnit != null)
            {
                winterSupplyTemperature = airHandlingUnit.winterSupplyTemperature;
                summerSupplyTemperature = airHandlingUnit.summerSupplyTemperature;
            }
        }

        /// <summary>
        /// Constructor that takes a GUID and a name parameter and creates a new object from them
        /// </summary>
        /// <param name="guid">The GUID of the air handling unit</param>
        /// <param name="name">The name of the air handling unit</param>
        /// <param name="summerSupplyTemperature">Summer Supply Temperature [°C]</param>
        /// <param name="winterSupplyTemperature">Winter Supply Temperature [°C]</param>
        public AirHandlingUnit(Guid guid, string name, double summerSupplyTemperature, double winterSupplyTemperature)
            : base(guid, name)
        {
            this.winterSupplyTemperature = winterSupplyTemperature;
            this.summerSupplyTemperature = summerSupplyTemperature;
        }

        /// <summary>
        /// Overrides the SAMObject method to create a new object from a JObject
        /// </summary>
        /// <param name="jObject">The JObject containing the air handling unit data</param>
        /// <returns>True if the deserialization is successful, false otherwise</returns>
        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("SummerSupplyTemperature"))
            {
                summerSupplyTemperature = jObject.Value<double>("SummerSupplyTemperature");
            }

            if (jObject.ContainsKey("WinterSupplyTemperature"))
            {
                winterSupplyTemperature = jObject.Value<double>("WinterSupplyTemperature");
            }

            return true;
        }

        /// <summary>
        /// Summer Supply Temperature [°C]
        /// </summary>
        public double SummerSupplyTemperature
        {
            get
            {
                return summerSupplyTemperature;
            }

            set
            {
                summerSupplyTemperature = value;
            }
        }

        /// <summary>
        /// Winter Supply Temperature [°C]
        /// </summary>
        public double WinterSupplyTemperature
        {
            get
            {
                return winterSupplyTemperature;
            }

            set
            {
                winterSupplyTemperature = value;
            }
        }

        /// <summary>
        /// Overrides the SAMObject method to convert the object to a JObject
        /// </summary>
        /// <returns>A JObject containing the air handling unit data</returns>
        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(!double.IsNaN(summerSupplyTemperature))
            {
                jObject.Add("SummerSupplyTemperature", summerSupplyTemperature);
            }

            if (!double.IsNaN(winterSupplyTemperature))
            {
                jObject.Add("WinterSupplyTemperature", winterSupplyTemperature);
            }

            return jObject;
        }
    }
}
