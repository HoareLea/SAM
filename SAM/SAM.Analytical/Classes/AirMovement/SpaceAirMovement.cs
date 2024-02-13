using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceAirMovement : SAMObject, IAirMovementObject
    {
        private Profile profile;
        private double airFlow;
        private string from;
        private string to;
        
        public SpaceAirMovement(string name, double airFlow, Profile profile, string from, string to)
            : base(name)
        {
            this.profile = profile == null ? null : new Profile(profile);
            this.airFlow = airFlow;
            this.from = from;
            this.to = to;
        }

        public SpaceAirMovement(string name, double airflow, string from, string to)
            : base(name)
        {
            this.airFlow = airflow;
            this.profile = new Profile(name, ProfileType.Ventilation, new double[] { 1 });
            this.from = from;
            this.to = to;
        }

        public SpaceAirMovement(SpaceAirMovement spaceAirMovement)
            : base(spaceAirMovement)
        {
            if(spaceAirMovement != null)
            {
                profile = spaceAirMovement.profile == null ? null : new Profile(spaceAirMovement.profile);
                airFlow = spaceAirMovement.airFlow;
                from = spaceAirMovement.from;
                to = spaceAirMovement.to;
            }
        }

        public SpaceAirMovement(JObject jObject)
            : base(jObject)
        {

        }

        public Profile Profile
        {
            get
            {
                return profile == null ? null : new Profile(profile);
            }
        }

        public double AirFlow
        {
            get
            {
                return airFlow;
            }
        }

        public string From
        {
            get
            {
                return from;
            }
        }

        public string To
        {
            get
            {
                return to;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("AirFlow"))
            {
                airFlow = jObject.Value<double>("AirFlow");
            }

            if (jObject.ContainsKey("Profile"))
            {
                profile = new Profile(jObject.Value<JObject>("Profile"));
            }

            if (jObject.ContainsKey("From"))
            {
                from = jObject.Value<string>("From");
            }

            if (jObject.ContainsKey("To"))
            {
                to = jObject.Value<string>("To");
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

            if(!double.IsNaN(airFlow))
            {
                jObject.Add("AirFlow", airFlow);
            }

            if (profile != null)
            {
                jObject.Add("Profile", profile.ToJObject());
            }

            if (from != null)
            {
                jObject.Add("From", from);
            }

            if (to != null)
            {
                jObject.Add("To", to);
            }

            return jObject;
        }
    }
}