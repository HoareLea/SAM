using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SAM.Core
{
    public class GuidCollection : Collection<Guid>, ISAMObject
    {
        private string name;
        private Guid guid;
        private List<ParameterSet> parameterSets;

        public GuidCollection(Guid guid)
        {
            Add(guid);
        }

        public GuidCollection(IEnumerable<Guid> guids)
        {
            if (guids == null)
                return;

            foreach (Guid guid in guids)
                Add(guid);
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Guid Guid
        {
            get
            {
                return guid;
            }
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            name = Query.Name(jObject);
            guid = Query.Guid(jObject);
            parameterSets = Create.ParameterSets(jObject.Value<JArray>("ParameterSets"));

            if(jObject.ContainsKey("Collection"))
            {
                foreach (JToken jToken in jObject.Value<JArray>("Collection"))
                    Add(Query.Guid(jToken));
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            if (name != null)
                jObject.Add("Name", name);

            jObject.Add("Guid", guid);

            if (parameterSets != null)
                jObject.Add("ParameterSets", Create.JArray(parameterSets));

            JArray jArray = new JArray();
            foreach (Guid guid in this)
                jArray.Add(guid);

            jObject.Add("Collection", jArray);

            return jObject;
        }
    }
}