using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;


namespace SAM.Core
{
    public class ParameterSet : ISAMObject
    {
        private string name;
        private Guid guid;
        private Dictionary<string, object> dictionary;

        public ParameterSet(string name)
        {
            this.name = name;
            dictionary = new Dictionary<string, object>();
        }

        public ParameterSet(Guid guid, string name)
        {
            this.name = name;
            this.guid = guid;
            dictionary = new Dictionary<string, object>();
        }

        public ParameterSet(Assembly assembly)
        {
            name = assembly.FullName;
            guid = Query.Guid(assembly);
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

        public Guid Guid
        {
            get
            {
                return guid;
            }
        }

        public bool Add(string name, string value)
        {
            if (dictionary == null || name == null)
                return false;
            
            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, double value)
        {
            if (dictionary == null || name == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, int value)
        {
            if (dictionary == null || name == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, bool value)
        {
            if (dictionary == null || name == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, IJSAMObject value)
        {
            if (dictionary == null || name == null)
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
            if (dictionary == null || name == null ||!dictionary.ContainsKey(name))
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Modify(string name, double value)
        {
            if (dictionary == null || name == null || !dictionary.ContainsKey(name))
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Contains(string name)
        {
            if (dictionary == null || name == null)
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

        public IJSAMObject ToSAMObject<T>(string name) where T: IJSAMObject
        {
            IJSAMObject result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return null;

            if (!(result is T))
                return null;

            return (T)(object)result;
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
            guid = Query.Guid(jObject);

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
                    case JTokenType.Object:
                        dictionary[jObject_Temp.Value<string>("Name")] = Create.IJSAMObject((JObject)jToken);
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
            
            jObject.Add("Guid", guid);

            JArray jArray = new JArray();
            foreach (KeyValuePair <string, object> keyValuePair in dictionary)
            {
                JObject jObject_Temp = new JObject();
                jObject_Temp.Add("Name", keyValuePair.Key);
                if (keyValuePair.Value != null)
                {
                    if (keyValuePair.Value is IJSAMObject)
                        jObject_Temp.Add("Value", ((IJSAMObject)keyValuePair.Value).ToJObject());
                    else
                        jObject_Temp.Add("Value", keyValuePair.Value as dynamic);
                }
                    

                jArray.Add(jObject_Temp);
            }

            if (jArray.Count != 0)
                jObject.Add("Parameters", jArray);

            return jObject;
        }
    }
}
