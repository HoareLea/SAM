using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class CaseData : IJSAMObject, IAnalyticalObject
    {
        private string name;

        public CaseData(string name)
        {
            this.name = name;
        }

        public CaseData(JObject jObject)
        {
            FromJObject(jObject);
        }

        public CaseData(CaseData caseData)
        {
            if(caseData != null)
            {
                name = caseData.name;
            }
        }

        public string Name
        {
            get 
            { 
                return name; 
            }
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Name"))
            {
                name = jObject.Value<string>("Name");
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (name != null)
            {
                jObject.Add("Name", name);
            }

            return jObject;
        }
    }
}