using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Reflection;

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
                return id;
            }
        }

        public ParameterSet GetParameterSet(string name)
        {
            return Query.ParameterSet(parameterSets, name);
        }

        public ParameterSet GetParameterSet(Assembly assembly)
        {
            return Query.ParameterSet(parameterSets, assembly);
        }

        public bool Add(ParameterSet parameterSet)
        {
            if (parameterSet == null)
                return false;

            if (parameterSets == null)
                parameterSets = new List<ParameterSet>();

            return Modify.Add(parameterSets, parameterSet);
        }

        public List<ParameterSet> GetParamaterSets()
        {
            if (parameterSets == null)
                return null;
            else
                return new List<ParameterSet>(parameterSets);
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