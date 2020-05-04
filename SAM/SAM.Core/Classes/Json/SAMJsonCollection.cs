using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace SAM.Core
{
    public class SAMJsonCollection<T> : Collection<T> where T : IJSAMObject
    {
        public SAMJsonCollection(string path)
        {
            FromJson(path);
        }

        public SAMJsonCollection(T t)
        {
            Add(t);
        }

        public SAMJsonCollection(IEnumerable<T> ts)
        {
            if (ts == null)
                return;

            foreach (T t in ts)
                Add(t);
        }

        public bool FromJson(string path)
        {
            JArray jArray = null;
            using (StreamReader streamReader = File.OpenText(path))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    JToken jToken = JToken.ReadFrom(reader);
                    if (jToken is JObject)
                        jArray = new JArray() { jToken };
                    else if (jToken is JArray)
                        jArray = (JArray)jToken;
                }
            }

            return FromJArray(jArray);
        }

        public bool ToJson(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            File.WriteAllText(path, ToJArray().ToString());
            return true;
        }

        public JArray ToJArray()
        {
            JArray jArray = new JArray();
            foreach (IJSAMObject jSAMObject in this)
                jArray.Add(jSAMObject.ToJObject());

            return jArray;
        }

        public bool FromJArray(JArray jArray)
        {
            if (jArray == null)
                return false;

            foreach (JObject jObject in jArray)
                Add(Create.IJSAMObject<T>(jObject));

            return true;
        }
    }
}