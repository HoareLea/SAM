using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class SpaceAirMovement : SAMObject, IAirMovementObject
    {
        private Profile airflow;
        private string from;
        private string to;
        
        public SpaceAirMovement(string name, Profile airflow, string from, string to)
            : base(name)
        {
            this.airflow = airflow == null ? null : new Profile(airflow);
            this.from = from;
            this.to = to;
        }

        public SpaceAirMovement(string name, double airflow, string from, string to)
            : base(name)
        {
            this.airflow = new Profile(name, ProfileType.Ventilation, new double[] { airflow });
            this.from = from;
            this.to = to;
        }

        public SpaceAirMovement(SpaceAirMovement spaceAirMovement)
            : base(spaceAirMovement)
        {
            if(spaceAirMovement != null)
            {
                airflow = spaceAirMovement.airflow == null ? null : new Profile(spaceAirMovement.airflow);
                from = spaceAirMovement.from;
                to = spaceAirMovement.to;
            }
        }

        public SpaceAirMovement(JObject jObject)
            : base(jObject)
        {

        }

        public Profile Airflow
        {
            get
            {
                return airflow == null ? null : new Profile(airflow);
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

            if(jObject.ContainsKey("Airflow"))
            {
                airflow = new Profile(jObject.Value<JObject>("Airflow"));
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

            if(airflow != null)
            {
                jObject.Add("Airflow", airflow.ToJObject());
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