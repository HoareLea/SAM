using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class ParameterSet
    {
        private string name;
        private Dictionary<string, object> dictionary;

        public ParameterSet(string name)
        {
            this.name = name;
            dictionary = new Dictionary<string, object>();
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
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return null;

            return result;
        }

        public double ToDouble(string name)
        {
            double result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return double.NaN;

            return result;
        }

        public double ToInt(string name)
        {
            int result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return int.MinValue;

            return result;
        }

        public bool ToBool(string name)
        {
            bool result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return false;

            return result;
        }

        public object ToObject(string name)
        {
            object result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return null;

            return result;
        }

        public Type GetType(string name)
        {
            object result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
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
                dictionary[keyValuePair.Key] = keyValuePair.Value;

            return parameterSet;
        }
    }
}
