using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class ParameterSet : IJSAMObject
    {
        private string name;
        private Dictionary<string, object> dictionary;

        public ParameterSet(string name)
        {
            this.name = name;
            dictionary = new Dictionary<string, object>();
        }

        public ParameterSet(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public bool Add(string name, string value)
        {
            if (dictionary == null)
                return false;
            
            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, double value)
        {
            if (dictionary == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, int value)
        {
            if (dictionary == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, bool value)
        {
            if (dictionary == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Remove(string name)
        {
            if (dictionary == null || name == null)
                return false;

            return dictionary.Remove(name);
        }

        public bool Modify(string name, string value)
        {
            if (dictionary == null || !dictionary.ContainsKey(name))
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Modify(string name, double value)
        {
            if (dictionary == null || !dictionary.ContainsKey(name))
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Contains(string name)
        {
            if (dictionary == null)
                return false;

            return dictionary.ContainsKey(name);
        }

        public string ToString(string name)
        {
            string result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return null;

            return result;
        }

        public double ToDouble(string name)
        {
            double result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return double.NaN;

            return result;
        }

        public double ToInt(string name)
        {
            int result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return int.MinValue;

            return result;
        }

        public bool ToBool(string name)
        {
            bool result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return false;

            return result;
        }

        public object ToObject(string name)
        {
            object result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return null;

            return result;
        }

        public Type GetType(string name)
        {
            object result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return null;

            if (result == null)
                return null;

            return result.GetType();
        }

        public IEnumerable<string> Names
        {
            get
            {
                return dictionary.Keys;
            }
        }

        public ParameterSet Clone()
        {
            ParameterSet parameterSet = new ParameterSet(name);
            foreach (KeyValuePair<string, object> keyValuePair in dictionary)
                parameterSet.dictionary[keyValuePair.Key] = keyValuePair.Value;

            return parameterSet;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            dictionary = new Dictionary<string, object>();
            
            name = Query.Name(jObject);

            JArray jArray = jObject.Value<JArray>("Parameters");
            if (jArray == null)
                return true;

            foreach (JObject jObject_Temp in jArray)
            {
                JToken jToken = jObject_Temp.GetValue("Value");
                if (jToken == null)
                    continue;

                switch (jToken.Type)
                {
                    case JTokenType.String:
                        dictionary[jObject_Temp.Value<string>("Name")] = jToken.Value<string>();
                        break;
                    case JTokenType.Float:
                        dictionary[jObject_Temp.Value<string>("Name")] = jToken.Value<double>();
                        break;
                    case JTokenType.Integer:
                        dictionary[jObject_Temp.Value<string>("Name")] = jToken.Value<int>();
                        break;
                    case JTokenType.Boolean:
                        dictionary[jObject_Temp.Value<string>("Name")] = jToken.Value<bool>();
                        break;
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", GetType().FullName);
            if (name != null)
                jObject.Add("Name", name);

            JArray jArray = new JArray();
            foreach (KeyValuePair <string, object> keyValuePair in dictionary)
            {
                JObject jObject_Temp = new JObject();
                jObject_Temp.Add("Name", keyValuePair.Key);
                if (keyValuePair.Value != null)
                    jObject_Temp.Add("Value", keyValuePair.Value as dynamic);

                jArray.Add(jObject_Temp);
            }

            if (jArray.Count != 0)
                jObject.Add("Parameters", jArray);

            return jObject;
        }
    }
}
