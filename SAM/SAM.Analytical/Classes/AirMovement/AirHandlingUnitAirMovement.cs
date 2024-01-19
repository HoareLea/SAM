using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class AirHandlingUnitAirMovement : SAMObject, IAirMovementObject
    {
        private Profile heating;
        private Profile cooling;
        private Profile humidification;
        private Profile dehumidification;
        
        public AirHandlingUnitAirMovement(string name)
            : base(name)
        {

        }

        public AirHandlingUnitAirMovement(string name, Profile heating, Profile cooling, Profile humidification, Profile dehumidification)
            : base(name)
        {
            this.heating = heating == null ? null : new Profile(heating);
            this.cooling = cooling == null ? null : new Profile(cooling);
            this.humidification = humidification == null ? null : new Profile(humidification);
            this.dehumidification = dehumidification == null ? null : new Profile(dehumidification);
        }

        public AirHandlingUnitAirMovement(AirHandlingUnitAirMovement airHandlingUnitAirMovement)
            : base(airHandlingUnitAirMovement)
        {
            if(airHandlingUnitAirMovement != null)
            {
                heating = airHandlingUnitAirMovement.heating == null ? null : new Profile(airHandlingUnitAirMovement.heating);
                cooling = airHandlingUnitAirMovement.cooling == null ? null : new Profile(airHandlingUnitAirMovement.cooling);
                humidification = airHandlingUnitAirMovement.humidification == null ? null : new Profile(airHandlingUnitAirMovement.humidification);
                dehumidification = airHandlingUnitAirMovement.dehumidification == null ? null : new Profile(airHandlingUnitAirMovement.dehumidification);
            }
        }

        public AirHandlingUnitAirMovement(JObject jObject)
            : base(jObject)
        {

        }

        public Profile Heating
        {
            get
            {
                return heating == null ? null : new Profile(heating);
            }
        }

        public Profile Cooling
        {
            get
            {
                return cooling == null ? null : new Profile(cooling);
            }
        }

        public Profile Humidification
        {
            get
            {
                return humidification == null ? null : new Profile(humidification);
            }
        }

        public Profile Dehumidification
        {
            get
            {
                return dehumidification == null ? null : new Profile(dehumidification);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Heating"))
            {
                heating = new Profile(jObject.Value<JObject>("Heating"));
            }

            if (jObject.ContainsKey("Cooling"))
            {
                cooling = new Profile(jObject.Value<JObject>("Cooling"));
            }

            if (jObject.ContainsKey("Humidification"))
            {
                humidification = new Profile(jObject.Value<JObject>("Humidification"));
            }

            if (jObject.ContainsKey("Dehumidification"))
            {
                dehumidification = new Profile(jObject.Value<JObject>("Dehumidification"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return null;
            }

            if(heating != null)
            {
                jObject.Add("Heating", heating.ToJObject());
            }

            if (cooling != null)
            {
                jObject.Add("Cooling", cooling.ToJObject());
            }

            if (humidification != null)
            {
                jObject.Add("Humidification", humidification.ToJObject());
            }

            if (dehumidification != null)
            {
                jObject.Add("Dehumidification", dehumidification.ToJObject());
            }

            return jObject;
        }
    }
}