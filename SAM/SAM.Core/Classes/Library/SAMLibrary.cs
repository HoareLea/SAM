using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAM.Core
{
    public abstract class SAMLibrary<T>: SAMObject, ISAMLibrary where T: IJSAMObject
    {
        private Dictionary<string, T> objects;

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

        public SAMLibrary(SAMLibrary<T> sAMLibrary)
            : base(sAMLibrary)
        {
            if (sAMLibrary.objects != null)
            {
                objects = new Dictionary<string, T>();

                foreach (T jSAMObject in sAMLibrary.objects.Values)
                    Add(jSAMObject);
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Objects"))
            {
                List<T> jSAMObjects = Create.IJSAMObjects<T>(jObject.Value<JArray>("Objects"));
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

        public bool Add(T jSAMObject)
        {
            if (jSAMObject == null)
                return false;

            if (!IsValid(jSAMObject))
                return false;

            string uniqueId = GetUniqueId(jSAMObject);
            if (uniqueId == null)
                return false;

            if (objects == null)
                objects = new Dictionary<string, T>();

            objects[uniqueId] = jSAMObject.Clone();
            return true;
        }

        public bool Contains(T jSAMObject)
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

        public bool Update(T jSAMObject)
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

        public bool Remove(T jSAMObject)
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

        public bool RemoveAll()
        {
            if(objects == null || objects.Count == 0)
            {
                return false;
            }

            objects = new Dictionary<string, T>();
            return true;
        }

        public T GetObject(string uniqueId)
        {
            if (uniqueId == null || objects == null || objects.Count == 0)
                return default;

            T result;

            objects.TryGetValue(uniqueId, out result);

            return result.Clone();
        }

        public W GetObject<W>(string uniqueId) where W: T
        {
            T result = GetObject(uniqueId);

            if (result is W)
                return (W)result;

            return default;
        }

        public List<T> GetObjects()
        {
            return objects?.Values?.ToList().ConvertAll(x => x.Clone());
        }

        public List<W> GetObjects<W>() where W: T
        {
            if (objects == null)
                return null;

            List<W> result = new List<W>();
            foreach (T jSAMObject in objects.Values)
                if (jSAMObject is W)
                    result.Add((W)jSAMObject.Clone());
            
            return result;
        }

        public List<W> GetObjects<W>(params Func<W, bool>[] functions) where W : T
        {
            List<W> objects = GetObjects<W>();
            objects.Filter(out List<W> @in, out List<W> @out, functions);

            return @in;
        }

        public Type GenericType
        {
            get
            {
                return typeof(T);
            }
        }
        
        public List<T> GetObjects(string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (text == null)
                return null;

            List<T> result = new List<T>();
            foreach (KeyValuePair<string, T> keyValuePair in objects)
                if (Query.Compare(keyValuePair.Key, text, textComparisonType, caseSensitive))
                    result.Add(keyValuePair.Value.Clone());

            return result;
        }

        public List<W> GetObjects<W>(string text, TextComparisonType textComparisonType, bool caseSensitive = true) where W: T
        {
            List<T> jSAMObjects = GetObjects(text, textComparisonType, caseSensitive);
            if (jSAMObjects == null)
                return null;

            List<W> result = new List<W>();
            foreach (T jSAMObject in jSAMObjects)
                if (jSAMObject is W)
                    result.Add((W)jSAMObject);

            return result;
        }

        public virtual string GetUniqueId(T jSAMObject)
        {
            return jSAMObject.GetHashCode().ToString();
        }

        public virtual bool IsValid(T jSAMObject)
        {
            if (jSAMObject == null)
                return false;

            return true;
        }

        public bool Replace(string uniqueId, T jSAMObject)
        {
            if(uniqueId == null || jSAMObject == null || objects == null)
            {
                return false;
            }

            if(!IsValid(jSAMObject))
            {
                return false;
            }

            if(!objects.ContainsKey(uniqueId))
            {
                return false;
            }

            string uniqueId_New = GetUniqueId(jSAMObject);
            if(uniqueId_New == null)
            {
                return false;
            }

            if(uniqueId != uniqueId_New)
            {
                objects.Remove(uniqueId);
                uniqueId = uniqueId_New;
            }

            objects[uniqueId] = jSAMObject;
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
                List<T> jSAMObjects = Create.IJSAMObjects<T>((JArray)jToken);
                if (jSAMObjects == null)
                    return false;

                if (objects == null)
                    objects = new Dictionary<string, T>();
                jSAMObjects.ForEach(x => Add(x));
                return true;
            }
            

            if(jToken is JObject)
            {
                IJSAMObject IJSAMObject = Create.IJSAMObject((JObject)jToken);
                if (IJSAMObject == null)
                    return false;

                if(IJSAMObject is SAMLibrary<T>)
                {
                    IEnumerable<T> jSAMObjects = ((SAMLibrary<T>)IJSAMObject).objects?.Values;
                    if (jSAMObjects != null && jSAMObjects.Count() > 0)
                        foreach (T jSAMObject in jSAMObjects)
                            Add(jSAMObject);

                    return true;
                }
                else if(IJSAMObject is T)
                {
                    return Add((T)IJSAMObject);
                }
            }

            return false;
        }
    }
}
