using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public abstract class SAMLibrary: SAMObject
    {
        private Dictionary<string, IJSAMObject> objects;

        public SAMLibrary(string name)
            : base(name)
        {

        }

        public SAMLibrary(Guid guid, string name)
            : base(guid, name)
        {

        }

        public SAMLibrary(JObject jObject)
            : base(jObject)
        {
        }

        public SAMLibrary(SAMLibrary sAMLibrary)
            : base(sAMLibrary)
        {
            if (sAMLibrary.objects != null)
            {
                objects = new Dictionary<string, IJSAMObject>();

                foreach (IJSAMObject jSAMObject in objects.Values)
                    Add(jSAMObject);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Objects"))
            {
                List<IJSAMObject> jSAMObjects = Create.IJSAMObjects<IJSAMObject>(jObject.Value<JArray>("Objects"));
                if (jSAMObjects != null && jSAMObjects.Count != 0)
                    jSAMObjects.ForEach(x => Add(x));
            }
                
            return true;
        }

        public bool Add(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return false;

            if (!IsValid(jSAMObject))
                return false;

            string uniqueId = GetUniqueId(jSAMObject);
            if (uniqueId == null)
                return false;

            if (objects == null)
                objects = new Dictionary<string, IJSAMObject>();

            objects[uniqueId] = jSAMObject.Clone();
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (objects != null && objects.Count != 0)
                jObject.Add("Objects", Create.JArray(objects.Values));

            return jObject;
        }

        public virtual string GetUniqueId(IJSAMObject jSAMObject)
        {
            return jSAMObject.GetHashCode().ToString();
        }

        public virtual bool IsValid(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return false;

            return true;
        }
    }
}
