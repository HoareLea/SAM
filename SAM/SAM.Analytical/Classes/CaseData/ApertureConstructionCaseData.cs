using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class ApertureConstructionCaseData : BuiltInCaseData
    {
        private ApertureConstruction apertureConstruction;

        public ApertureConstructionCaseData(ApertureConstruction apertureConstruction)
            : base(nameof(ApertureConstructionCaseData))
        {
            this.apertureConstruction = apertureConstruction?.Clone();
        }

        public ApertureConstructionCaseData(JObject jObject)
            : base(jObject)
        {

        }

        public ApertureConstructionCaseData(ApertureConstructionCaseData apertureConstructionCaseData)
            : base(apertureConstructionCaseData)
        {
            if(apertureConstructionCaseData != null)
            {
                apertureConstruction = apertureConstructionCaseData.apertureConstruction?.Clone();
            }
        }

        public ApertureConstruction ApertureConstruction
        {
            get 
            { 
                return apertureConstruction?.Clone(); 
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return false;
            }

            if(jObject.ContainsKey("ApertureConstruction"))
            {
                apertureConstruction = Core.Query.IJSAMObject<ApertureConstruction>(jObject.Value<JObject>("ApertureConstruction"));
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

            if(apertureConstruction is not null)
            {
                result.Add("ApertureConstruction", apertureConstruction.ToJObject());
            }

            return result;
        }
    }
}