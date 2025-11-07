using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public abstract class BuiltInCaseData : CaseData
    {
        public BuiltInCaseData(string name)
            :base(name)
        {

        }

        public BuiltInCaseData(JObject jObject)
            :base(jObject)
        {

        }

        public BuiltInCaseData(BuiltInCaseData builtInCaseData)
            : base(builtInCaseData)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            return base.FromJObject(jObject);
        }

        public override JObject ToJObject()
        {
            return base.ToJObject();
        }
    }
}