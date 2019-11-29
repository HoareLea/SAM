using System;
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

        public bool Update(string name, string value)
        {
            if (dictionary == null || !dictionary.ContainsKey(name))
                return false;
            
            dictionary[name] = value;
            return true;
        }

        public bool Update(string name, double value)
        {
            if (dictionary == null || !dictionary.ContainsKey(name))
                return false;

            dictionary[name] = value;
            return true;
        }

        public string AsString(string name)
        {
            string result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return null;

            return result;
        }

        public double AsDouble(string name)
        {
            double result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return double.NaN;

            return result;
        }

        public double AsInt(string name)
        {
            int result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return int.MinValue;

            return result;
        }

        public bool AsBool(string name)
        {
            bool result;
            if (!DictionaryUtils.TryGetValue(dictionary, name, out result))
                return false;

            return result;
        }
    }
}
