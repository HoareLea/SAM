using Newtonsoft.Json.Linq;

using System.Collections.Generic;

namespace SAM.Core
{
    public class IntegerId : IJSAMObject
    {
        private int id;
        private List<ParameterSet> parameterSets;

        public IntegerId(int id)
        {
            this.id = id;
        }

        public int Id
        {
            get
            {
                return Id;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            id = jObject.Value<int>("Id");
            parameterSets = Create.ParameterSets(jObject.Value<JArray>("ParameterSets"));
            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            jObject.Add("Id", id);

            if (parameterSets != null)
                jObject.Add("ParameterSets", Create.JArray(parameterSets));

            return jObject;
        }
    }
}
