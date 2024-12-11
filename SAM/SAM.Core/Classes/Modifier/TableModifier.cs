using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class TableModifier : SimpleModifier
    {
        public bool Extrapolate { get; set; }

        private SortedDictionary<int, string> headers = new SortedDictionary<int, string>();
        private List<SortedDictionary<int, double>> values = new List<SortedDictionary<int, double>>();

        public TableModifier(ArithmeticOperator arithmeticOperator, IEnumerable<string> headers)
        {
            ArithmeticOperator = arithmeticOperator;
            Headers = headers;
        }

        public TableModifier(TableModifier tableModifier)
            : base(tableModifier)
        {
            if(tableModifier != null)
            {
                if(tableModifier.headers != null)
                {
                    foreach(KeyValuePair<int, string> keyValuePair in tableModifier.headers)
                    {
                        headers[keyValuePair.Key] = keyValuePair.Value;
                    }
                }

                if(tableModifier.values != null)
                {
                    foreach(SortedDictionary<int, double> sortedDictionary in tableModifier.values)
                    {
                        if(sortedDictionary == null)
                        {
                            continue;
                        }

                        SortedDictionary<int, double> sortedDictionary_New = new SortedDictionary<int, double>();
                        foreach(KeyValuePair<int, double> keyValuePair in sortedDictionary)
                        {
                            sortedDictionary_New[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }
                }
            }
        }

        public TableModifier(JObject jObject)
            :base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("Extrapolate"))
            {
                Extrapolate = jObject.Value<bool>("Extrapolate");
            }

            if(jObject.ContainsKey("Headers"))
            {
                JArray jArray = jObject.Value<JArray>("Headers");
                if(jArray != null)
                {
                    headers = new SortedDictionary<int, string>();
                    foreach(JArray jArray_Header in jArray)
                    {
                        headers[((JValue)jArray_Header[0]).Value<int>()] = ((JValue)jArray_Header[1]).Value<string>();
                    }
                }
            }

            if (jObject.ContainsKey("Values"))
            {
                JArray jArray = jObject.Value<JArray>("Values");
                if (jArray != null)
                {
                    values = new List<SortedDictionary<int, double>>();
                    foreach (JArray jArray_Row in jArray)
                    {
                        SortedDictionary<int, double> sortedDictionary = new SortedDictionary<int, double>();
                        foreach (JArray jArray_Values in jArray_Row)
                        {
                            sortedDictionary[((JValue)jArray_Values[0]).Value<int>()] = ((JValue)jArray_Values[1]).Value<double>();
                        }

                        values.Add(sortedDictionary);
                    }
                }
            }

            return result;
        }

        public IEnumerable<string> Headers
        {
            get
            {
                if(headers == null)
                {
                    return null;
                }

                List<string> result = new List<string>();
                if(headers.Count == 0)
                {
                    return result;
                }

                for(int i = headers.Keys.First(); i <= headers.Keys.Max(); i++)
                {
                    if(!headers.TryGetValue(i, out string header))
                    {
                        header = null;
                    }

                    result.Add(header);
                }

                return result;
            }

            set
            {
                headers.Clear();
                if(value == null)
                {
                    return;
                }

                for (int i = 0; i < value.Count(); i++)
                {
                    headers[i] = value.ElementAt(i);
                }
            }
        }

        public int GetHeaderIndex(string name)
        {
            if(headers == null || headers.Count == 0)
            {
                return -1;
            }

            foreach(KeyValuePair<int, string> keyValuePair in headers)
            {
                if(name == keyValuePair.Value)
                {
                    return keyValuePair.Key;
                }
            }

            return -1;
        }

        public bool AddValues(IDictionary<int, double> values)
        {
            if(values == null)
            {
                return false;
            }

            SortedDictionary<int, double> values_Temp = new SortedDictionary<int, double>();
            foreach(KeyValuePair<int, string> keyValuePair in headers)
            {
                if(!values.TryGetValue(keyValuePair.Key, out double value))
                {
                    continue;
                }

                values_Temp.Add(keyValuePair.Key, value);
            }

            this.values.Add(values_Temp);
            return true;
        }

        public bool AddValues(IDictionary<string, double> values, bool addMissingHeaders = false)
        {
            if(values == null)
            {
                return false;
            }

            SortedDictionary<int, double> values_Temp = new SortedDictionary<int, double>();
            foreach (KeyValuePair<string, double> keyValuePair in values)
            {
                int index = GetHeaderIndex(keyValuePair.Key);
                if(index == -1)
                {
                    if(!addMissingHeaders)
                    {
                        continue;
                    }

                    index = headers.Count == 0 ? 0 : headers.Keys.Last() + 1;
                    headers[index] = keyValuePair.Key;
                }

                values_Temp[index] = keyValuePair.Value;
            }

            return AddValues(values_Temp);
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            result.Add("Extrapolate", Extrapolate);

            if(headers != null)
            {
                JArray jArray = new JArray();
                foreach(KeyValuePair<int, string> keyValuePair in headers)
                {
                    jArray.Add(new JArray() { keyValuePair.Key, keyValuePair.Value });
                }

                result.Add("Headers", jArray);
            }

            if(values != null)
            {
                JArray jArray_Values = new JArray();
                foreach(SortedDictionary<int, double> sortedDictionary in values)
                {
                    JArray jArray_Row = new JArray();
                    foreach (KeyValuePair<int, double> keyValuePair in sortedDictionary)
                    {
                        jArray_Row.Add(new JArray() { keyValuePair.Key, keyValuePair.Value });
                    }
                    jArray_Values.Add(jArray_Row);
                }

                result.Add("Values", jArray_Values);
            }

            return result;
        }
    }
}