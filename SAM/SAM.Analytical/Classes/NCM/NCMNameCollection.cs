using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class NCMNameCollection : IEnumerable<NCMName>, IJSAMObject
    {
        private List<NCMName> nCMNames = new List<NCMName>();

        public NCMNameCollection()
        {

        }

        public NCMNameCollection(IEnumerable<NCMName> nCMNames)
        {
            if(nCMNames != null)
            {
                this.nCMNames = new List<NCMName>(nCMNames);
            }
        }

        public NCMNameCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool Add(NCMName nCMName)
        {
            if(nCMName == null)
            {
                return false;
            }

            nCMNames.Add(nCMName);
            return true;
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            nCMNames = new List<NCMName>();

            if(jObject.ContainsKey("NCMNames"))
            {
                JArray jArray = jObject.Value<JArray>("NCMNames");
                foreach(JObject jObject_NCMName in jArray)
                {
                    if(jObject_NCMName == null)
                    {
                        continue;
                    }

                    nCMNames.Add(new NCMName(jObject_NCMName));
                }

            }

            return true;
        }

        public IEnumerator<NCMName> GetEnumerator()
        {
            return nCMNames.GetEnumerator();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(nCMNames != null)
            {
                JArray jArray = new JArray();
                foreach(NCMName nCMName in nCMNames)
                {
                    if(nCMName == null)
                    {
                        continue;
                    }

                    jArray.Add(nCMName.ToJObject());
                }

                jObject.Add("NCMNames", jArray);
            }

            return jObject;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}