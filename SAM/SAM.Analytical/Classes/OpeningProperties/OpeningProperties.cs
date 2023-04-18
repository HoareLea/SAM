using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    /// <summary>
    /// Opening Properties
    /// </summary>
    public class OpeningProperties : ParameterizedSAMObject, ISingleOpeningProperties
    {
        public double Factor { get; set; } = 1;
        
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
                Factor = openingProperties.Factor;
                dischargeCoefficient = openingProperties.dischargeCoefficient;
            }
        }

        public OpeningProperties(IOpeningProperties openingProperties, double dischargeCoefficient)
            : base(openingProperties as ParameterizedSAMObject)
        {
            this.dischargeCoefficient = dischargeCoefficient;
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

            if (jObject.ContainsKey("Factor"))
            {
                Factor = jObject.Value<double>("Factor");
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

            if (!double.IsNaN(Factor))
            {
                jObject.Add("Factor", Factor);
            }

            return jObject;
        }

        public double GetDischargeCoefficient()
        {
            return dischargeCoefficient;
        }

        public double GetFactor()
        {
            return Factor;
        }
    }
}