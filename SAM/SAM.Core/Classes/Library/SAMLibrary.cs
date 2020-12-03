using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

                foreach (IJSAMObject jSAMObject in sAMLibrary.objects.Values)
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

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (objects != null && objects.Count != 0)
                jObject.Add("Objects", Create.JArray(objects.Values));

            return jObject;
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

        public bool Contains(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null || objects == null)
                return false;

            if (!IsValid(jSAMObject))
                return false;

            string uniqueId = GetUniqueId(jSAMObject);
            if (uniqueId == null)
                return false;

            return objects.ContainsKey(uniqueId);
        }

        public bool Update(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return false;

            if (objects == null || objects.Count == 0)
                return false;

            if (!IsValid(jSAMObject))
                return false;

            string uniqueId = GetUniqueId(jSAMObject);
            if (uniqueId == null)
                return false;

            if (!objects.ContainsKey(uniqueId))
                return false;

            objects[uniqueId] = jSAMObject.Clone();
            return true;
        }

        public bool Remove(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return false;

            if (objects == null || objects.Count == 0)
                return false;

            if (!IsValid(jSAMObject))
                return false;

            string uniqueId = GetUniqueId(jSAMObject);
            if (uniqueId == null)
                return false;

            return objects.Remove(uniqueId);
        }

        public IJSAMObject GetObject(string uniqueId)
        {
            if (uniqueId == null || objects == null || objects.Count == 0)
                return null;

            IJSAMObject result;

            objects.TryGetValue(uniqueId, out result);

            return result.Clone();
        }

        public T GetObject<T>(string uniqueId) where T: IJSAMObject
        {
            IJSAMObject result = GetObject(uniqueId);

            if (result is T)
                return (T)result;

            return default;
        }

        public List<IJSAMObject> GetObjects()
        {
            return objects?.Values?.ToList().ConvertAll(x => x.Clone());
        }

        public List<T> GetObjects<T>() where T: IJSAMObject
        {
            if (objects == null)
                return null;

            List<T> result = new List<T>();
            foreach (IJSAMObject jSAMObject in objects.Values)
                if (jSAMObject is T)
                    result.Add((T)jSAMObject.Clone());
            
            return result;
        }
        
        public List<IJSAMObject> GetObjects(string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (text == null)
                return null;

            List<IJSAMObject> result = new List<IJSAMObject>();
            foreach (KeyValuePair<string, IJSAMObject> keyValuePair in objects)
                if (Query.Compare(keyValuePair.Key, text, textComparisonType, caseSensitive))
                    result.Add(keyValuePair.Value.Clone());

            return result;
        }

        public List<T> GetObjects<T>(string text, TextComparisonType textComparisonType, bool caseSensitive = true) where T: IJSAMObject
        {
            List<IJSAMObject> jSAMObjects = GetObjects(text, textComparisonType, caseSensitive);
            if (jSAMObjects == null)
                return null;

            List<T> result = new List<T>();
            foreach (IJSAMObject jSAMObject in jSAMObjects)
                if (jSAMObject is T)
                    result.Add((T)jSAMObject);

            return result;
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

        public bool Write(string path)
        {
            return Query.Write(this, path);
        }

        public bool Append(string path)
        {
            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);

            JToken jToken = JToken.Parse(json);
            if (jToken == null)
                return false;

            if(jToken.Type == JTokenType.Array)
            {
                List<IJSAMObject> jSAMObjects = Create.IJSAMObjects<IJSAMObject>((JArray)jToken);
                if (jSAMObjects == null)
                    return false;

                if (objects == null)
                    objects = new Dictionary<string, IJSAMObject>();
                jSAMObjects.ForEach(x => Add(x));
                return true;
            }
            

            if(jToken is JObject)
            {
                IJSAMObject IJSAMObject = Create.IJSAMObject((JObject)jToken);
                if (IJSAMObject == null)
                    return false;

                if(IJSAMObject is SAMLibrary)
                {
                    IEnumerable<IJSAMObject> jSAMObjects = ((SAMLibrary)IJSAMObject).objects?.Values;
                    if (jSAMObjects != null && jSAMObjects.Count() > 0)
                        foreach (IJSAMObject jSAMObject in jSAMObjects)
                            Add(jSAMObject);

                    return true;
                }
                else
                {
                    return Add(IJSAMObject);
                }
            }

            return false;
        }
    }
}
