using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class ApertureCaseData : BuiltInCaseData
    {
        private List<double> ratios;

        public ApertureCaseData(IEnumerable<double> ratios)
            : base(nameof(ApertureCaseData))
        {
            this.ratios = ratios == null ? [] : [.. ratios];
        }

        public ApertureCaseData(JObject jObject)
            : base(jObject)
        {

        }

        public ApertureCaseData(ApertureCaseData apertureCaseData)
            : base(apertureCaseData)
        {
            if(apertureCaseData != null)
            {
                ratios = apertureCaseData.Ratios;
            }
        }

        public List<double> Ratios
        {
            get 
            { 
                return ratios?.ToList(); 
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return false;
            }

            if(jObject.ContainsKey("Ratios"))
            {
                ratios = [];
                JArray jArray = jObject.Value<JArray>("Ratios");
                foreach(double value in jArray)
                {
                    ratios.Add(value);
                }
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result is null)
            {
                return result;
            }

            if(ratios != null)
            {
                result.Add("Rations", new JArray(ratios));
            }

            return result;
        }
    }
}