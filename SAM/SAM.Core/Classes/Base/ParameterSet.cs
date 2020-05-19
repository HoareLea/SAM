using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public class ParameterSet : ISAMObject
    {
        private string name;
        private Guid guid;
        private Dictionary<string, object> dictionary;

        public ParameterSet(ParameterSet parameterSet)
        {
            name = parameterSet.name;
            guid = parameterSet.guid;

            if (parameterSet.dictionary != null)
            {
                dictionary = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> keyValuePair in parameterSet.dictionary)
                    dictionary[keyValuePair.Key] = keyValuePair.Value;
            }
        }

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
            name = Query.Name(assembly);
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

        public bool Add(string name, JObject value)
        {
            if (dictionary == null || name == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, DateTime value)
        {
            if (dictionary == null || name == null)
                return false;

            dictionary[name] = value;
            return true;
        }

        public bool Add(string name, System.Drawing.Color color)
        {
            if (dictionary == null || name == null)
                return false;

            dictionary[name] = new SAMColor(color);
            return true;
        }

        public bool Add(string name, SAMColor sAMColor)
        {
            if (dictionary == null || name == null)
                return false;

            dictionary[name] = sAMColor;
            return true;
        }

        public bool Copy(ParameterSet parameterSet)
        {
            if (parameterSet == null)
                return false;

            if (dictionary == null)
                dictionary = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> keyValuePair in parameterSet.dictionary)
                dictionary[keyValuePair.Key] = keyValuePair.Value;

            return true;
        }

        public void Clear()
        {
            if (dictionary != null)
                dictionary.Clear();
        }

        public bool Remove(string name)
        {
            if (dictionary == null || name == null)
                return false;

            return dictionary.Remove(name);
        }

        public bool Modify(string name, string value)
        {
            if (dictionary == null || name == null || !dictionary.ContainsKey(name))
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

        public int ToInt(string name)
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

        public DateTime ToDateTime(string name)
        {
            DateTime result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return result;

            return DateTime.MinValue;
        }

        public JObject ToJObject(string name)
        {
            JObject result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return result;

            return null;
        }

        public T ToSAMObject<T>(string name) where T : IJSAMObject
        {
            IJSAMObject result;
            if (!Query.TryGetValue(dictionary, name, out result))
                return default(T);

            if (!(result is T))
                return default(T);

            return (T)(object)result;
        }

        public System.Drawing.Color ToColor(string name)
        {
            SAMColor sAMColor = ToSAMObject<SAMColor>(name); 

            if (sAMColor == null)
                return System.Drawing.Color.Empty;

            return sAMColor.ToColor();
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
                if (dictionary == null)
                    return null;

                return dictionary.Keys;
            }
        }

        public ParameterSet Clone()
        {
            return new ParameterSet(this);
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

                    case JTokenType.Date:
                        dictionary[jObject_Temp.Value<string>("Name")] = jToken.Value<DateTime>();
                        break;

                    case JTokenType.Object:
                        JSAMObjectWrapper jSAMObjectWrapper = new JSAMObjectWrapper((JObject)jToken);
                        IJSAMObject jSAMObject = jSAMObjectWrapper.ToIJSAMObject();
                        if (jSAMObject == null)
                            dictionary[jObject_Temp.Value<string>("Name")] = jSAMObjectWrapper.ToJObject();
                        else
                            dictionary[jObject_Temp.Value<string>("Name")] = jSAMObject;
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
            foreach (KeyValuePair<string, object> keyValuePair in dictionary)
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