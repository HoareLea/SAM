using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    /// <summary>
    /// Opening Properties
    /// </summary>
    public class OpeningProperties : ParameterizedSAMObject, IOpeningProperties
    {
        private double dischargeCoefficient { get; set; }

        public OpeningProperties()
        {

        }

        public OpeningProperties(double dischargeCoefficient)
        {
            this.dischargeCoefficient = dischargeCoefficient;
        }

        public OpeningProperties(JObject jObject)
            :base(jObject)
        {
        }

        public OpeningProperties(OpeningProperties openingProperties)
            : base(openingProperties)
        {
            if(openingProperties != null)
            {
                dischargeCoefficient = openingProperties.dischargeCoefficient;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("DischargeCoefficient"))
            {
                dischargeCoefficient = jObject.Value<double>("DischargeCoefficient");
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if(!double.IsNaN(dischargeCoefficient))
            {
                jObject.Add("DischargeCoefficient", dischargeCoefficient);
            }

            return jObject;
        }

        public double GetDischargeCoefficient()
        {
            return dischargeCoefficient;
        }
    }
}