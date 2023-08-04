using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an air handling unit object in the analytical domain
    /// </summary>
    public class AirHandlingUnit : ComplexEquipment
    {
        /// <summary>
        /// Constructor that takes a name parameter and sets the base object's name
        /// </summary>
        /// <param name="name">The name of the air handling unit</param>
        public AirHandlingUnit(string name)
            : base(name)
        {

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

        }

        /// <summary>
        /// Constructor that takes a GUID and a name parameter and creates a new object from them
        /// </summary>
        /// <param name="guid">The GUID of the air handling unit</param>
        /// <param name="name">The name of the air handling unit</param>
        public AirHandlingUnit(Guid guid, string name)
            : base(guid, name)
        {

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

            // TODO: Implement specific deserialization logic for AirHandlingUnit properties

            return true;
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

            // TODO: Implement specific serialization logic for AirHandlingUnit properties

            return jObject;
        }
    }
}
