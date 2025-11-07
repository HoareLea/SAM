using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class CaseDataCollection : IJSAMObject, IAnalyticalObject, IEnumerable<CaseData>
    {
        private List<CaseData> caseDatas = [];

        public CaseDataCollection()
        {

        }

        public CaseDataCollection(CaseDataCollection caseDataCollection)
        {
            if (caseDataCollection != null)
            {
                foreach (CaseData caseData in caseDataCollection)
                {
                    if (caseData.Clone() is CaseData caseData_Temp)
                    {
                        this.caseDatas.Add(caseData_Temp);
                    }
                }
            }
        }

        public CaseDataCollection(IEnumerable<CaseData> caseDatas)
        {
            if(caseDatas != null)
            {
                foreach(CaseData caseData in caseDatas)
                {
                    if(caseData.Clone() is CaseData caseData_Temp)
                    {
                        this.caseDatas.Add(caseData_Temp);
                    }
                }
            }
        }

        public CaseDataCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("CaseDatas"))
            {
                JArray jArray = jObject.Value<JArray>("CaseDatas");
                foreach(JObject jObject_Temp in jArray)
                {
                    if(Core.Query.IJSAMObject<CaseData>(jObject_Temp) is CaseData caseData)
                    {
                        caseDatas.Add(caseData);
                    }
                }
            }

            return true;
        }

        public List<CaseData> Values
        {
            get
            {
                return caseDatas?.ConvertAll(x => x.Clone());
            }
        }

        public IEnumerator<CaseData> GetEnumerator()
        {
            return caseDatas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (caseDatas != null)
            {
                JArray jArray = [];
                foreach (CaseData caseData in caseDatas)
                {
                    jArray.Add(caseData.ToJObject());
                }

                jObject.Add("CaseDatas", jArray);
            }

            return jObject;
        }
    }
}