using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SAM.Core
{
    public class SAMCollection<T> : Collection<T>, ISAMObject where T : IJSAMObject
    {
        private string name;
        private Guid guid;
        private List<ParameterSet> parameterSets;

        public SAMCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMCollection(SAMCollection<T> sAMCollection)
        {
            name = sAMCollection.name;
            guid = sAMCollection.guid;
            parameterSets = sAMCollection.parameterSets?.Clone();

            foreach (T t in sAMCollection)
                Add(t);
        }
        
        public SAMCollection()
        {
            guid = Guid.NewGuid();
        }
        
        public SAMCollection(T t)
        {
            guid = Guid.NewGuid();
            Add(t);
        }

        public SAMCollection(IEnumerable<T> ts)
        {
            guid = Guid.NewGuid();

            if (ts == null)
                return;

            foreach (T t in ts)
                Add(t);
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
                foreach (JObject jObject_Collection in jObject.Value<JArray>("Collection"))
                    Add(Create.IJSAMObject<T>(jObject_Collection));
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
            foreach (T t in this)
                jArray.Add(t.ToJObject());

            jObject.Add("Collection", jArray);

            return jObject;
        }
    }
}