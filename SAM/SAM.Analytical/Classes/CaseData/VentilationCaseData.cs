using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public class VentilationCaseData : BuiltInCaseData
    {
        private double aCH;

        public VentilationCaseData(double aCH)
            : base(nameof(OpeningCaseData))
        {
            this.aCH = aCH;
        }

        public VentilationCaseData(JObject jObject)
            : base(jObject)
        {

        }

        public VentilationCaseData(VentilationCaseData ventilationCaseData)
            : base(ventilationCaseData)
        {
            if(ventilationCaseData != null)
            {
                aCH = ventilationCaseData.aCH;
            }
        }

        public double ACH
        {
            get 
            { 
                return aCH; 
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return false;
            }

            if(jObject.ContainsKey("ACH"))
            {
                aCH = jObject.Value<double>("ACH");
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

            if(!double.IsNaN(aCH))
            {
                result.Add("ACH", aCH);
            }

            return result;
        }
    }
}