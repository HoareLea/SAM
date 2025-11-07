using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class OpeningCaseData : BuiltInCaseData
    {
        private double openingAngle;

        public OpeningCaseData(double openingAngle)
            : base(nameof(OpeningCaseData))
        {
            this.openingAngle = openingAngle;
        }

        public OpeningCaseData(JObject jObject)
            : base(jObject)
        {

        }

        public OpeningCaseData(OpeningCaseData openingCaseData)
            : base(openingCaseData)
        {
            if(openingCaseData != null)
            {
                openingAngle = openingCaseData.openingAngle;
            }
        }

        public double OpeningAngle
        {
            get 
            { 
                return openingAngle; 
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return false;
            }

            if(jObject.ContainsKey("OpeningAngle"))
            {
                openingAngle = jObject.Value<double>("OpeningAngle");
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

            if(!double.IsNaN(openingAngle))
            {
                result.Add("OpeningAngle", openingAngle);
            }

            return result;
        }
    }
}