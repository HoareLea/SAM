using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class WindowSizeCaseData : BuiltInCaseData
    {
        private double apertureScaleFactor;

        public WindowSizeCaseData(double apertureScaleFactor)
            : base(nameof(WindowSizeCaseData))
        {
            this.apertureScaleFactor = apertureScaleFactor;
        }

        public WindowSizeCaseData(JObject jObject)
            : base(jObject)
        {

        }

        public WindowSizeCaseData(WindowSizeCaseData windowSizeCaseData)
            : base(windowSizeCaseData)
        {
            if(windowSizeCaseData != null)
            {
                apertureScaleFactor = windowSizeCaseData.apertureScaleFactor;
            }
        }

        public double ApertureScaleFactor
        {
            get 
            { 
                return apertureScaleFactor; 
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return false;
            }

            if(jObject.ContainsKey("ApertureScaleFactor"))
            {
                apertureScaleFactor = jObject.Value<double>("ApertureScaleFactor");
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

            if(!double.IsNaN(apertureScaleFactor))
            {
                result.Add("ApertureScaleFactor", apertureScaleFactor);
            }

            return result;
        }
    }
}