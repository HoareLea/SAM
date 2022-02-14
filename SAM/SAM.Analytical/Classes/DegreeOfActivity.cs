using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class DegreeOfActivity : SAMObject, IAnalyticalObject
    {
        private double sensible;
        private double latent;

        public DegreeOfActivity(DegreeOfActivity degreeOfActivity)
            : base(degreeOfActivity)
        {
            sensible = degreeOfActivity.sensible;
            latent = degreeOfActivity.latent;
        }

        public DegreeOfActivity(Guid guid, DegreeOfActivity degreeOfActivity)
        : base(guid, degreeOfActivity)
        {
            sensible = degreeOfActivity.sensible;
            latent = degreeOfActivity.latent;
        }

        public DegreeOfActivity(string name, double sensible, double latent)
            : base(name)
        {
            this.sensible = sensible;
            this.latent = latent;
        }

        public DegreeOfActivity(JObject jObject)
            : base(jObject)
        {
        }
        
        /// <summary>
        /// Dry (sensible) total heat emission [W/p]
        /// </summary>
        public double Sensible
        {
            get
            {
                return sensible;
            }
        }

        /// <summary>
        /// Humid (latent) heat emission [W/p]
        /// </summary>
        public double Latent
        {
            get
            {
                return latent;
            }
        }

        public double GetTotal()
        {
            if (double.IsNaN(sensible) && double.IsNaN(latent))
                return double.NaN;

            double result = 0;
            if (!double.IsNaN(sensible))
                result += sensible;

            if (!double.IsNaN(latent))
                result += latent;

            return result;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Sensible"))
                sensible = jObject.Value<double>("Sensible");
            else
                sensible = double.NaN;

            if (jObject.ContainsKey("Latent"))
                latent = jObject.Value<double>("Latent");
            else
                latent = double.NaN;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (sensible != double.NaN)
                jObject.Add("Sensible", sensible);

            if (latent != double.NaN)
                jObject.Add("Latent", latent);

            return jObject;
        }
    }
}