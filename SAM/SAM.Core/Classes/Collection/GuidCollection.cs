using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public class GuidCollection : SAMObject, IEnumerable<Guid>
    {
        private List<Guid> guids = new List<Guid>();

        public GuidCollection(Guid guid, string name)
            : base(guid, name)
        {

        }

        public GuidCollection(string name)
            : base(name)
        {

        }

        public GuidCollection(string name, ParameterSet parameterSet)
            : base(Guid.NewGuid(), name, new ParameterSet[] { parameterSet})
        {

        }
        
        public GuidCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public GuidCollection(GuidCollection guidCollection)
            : base(guidCollection)
        {

            if (guidCollection?.guids != null)
                guids = new List<Guid>(guidCollection.guids);
        }

        public GuidCollection()
            : base()
        {

        }

        public GuidCollection(IEnumerable<Guid> guids)
            : base()
        {
            foreach (Guid guid in guids)
                this.guids.Add(guid);
        }

        public virtual void Add(Guid guid)
        {
            guids.Add(guid);
        }

        public bool Remove(Guid guid)
        {
            return guids.Remove(guid);
        }

        public List<bool> Remove(IEnumerable<Guid> guids)
        {
            if (guids == null)
                return null;

            List<bool> result = new List<bool>();

            foreach (Guid guid in guids)
                result.Add(Remove(guid));

            return result;
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("Collection"))
            {
                guids = new List<Guid>();
                foreach (JToken jToken in jObject.Value<JArray>("Collection"))
                    guids.Add(Query.Guid(jToken));
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray = new JArray();
            foreach (Guid guid in this)
                jArray.Add(guid);

            jObject.Add("Collection", jArray);

            return jObject;
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return guids?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return guids?.GetEnumerator();
        }
    }
}