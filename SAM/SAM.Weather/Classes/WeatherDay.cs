using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public class WeatherDay : IJSAMObject
    {
        private Dictionary<string, double[]> dictionary;

        public WeatherDay()
        {
            dictionary = new Dictionary<string, double[]>();
        }
        
        public WeatherDay(WeatherDay weatherDay)
        {
            dictionary = new Dictionary<string, double[]>();
            foreach (KeyValuePair<string, double[]> keyValuePair in weatherDay.dictionary)
                dictionary[keyValuePair.Key] = (double[])keyValuePair.Value.Clone();
        }

        public WeatherDay(JObject jObject)
        {
            FromJObject(jObject);
        }

        public double this[string name, int index]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name) || index > 23)
                    return double.NaN;

                if (dictionary == null)
                    return double.NaN;

                double[] values = null;
                if (!dictionary.TryGetValue(name, out values) || values == null)
                    return double.NaN;

                return values[index];
            }
            set
            {
                if (string.IsNullOrWhiteSpace(name) || index > 23)
                    return;

                if (dictionary == null)
                    dictionary = new Dictionary<string, double[]>();

                double[] values = null;
                if (!dictionary.TryGetValue(name, out values))
                {
                    values = Enumerable.Repeat(double.NaN, 24).ToArray();
                    dictionary[name] = values;
                }

                values[index] = value;
            }
        }

        public double this[WeatherDataType weatherDataType, int index]
        {
            get
            {
                return this[weatherDataType.ToString(), index];
            }
            set
            {
                this[weatherDataType.ToString(), index] = value;
            }
        }

        public double[] this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name) || dictionary == null || dictionary.Count == 0)
                    return null;

                double[] values;
                if (!dictionary.TryGetValue(name, out values))
                    return null;

                return (double[])values.Clone();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(name))
                    return;

                if(value == null)
                {
                    if (dictionary != null)
                        dictionary.Remove(name);

                    return;
                }

                if (value.Length != 24)
                    return;

                if (dictionary == null)
                    dictionary = new Dictionary<string, double[]>();

                dictionary[name] = (double[])value.Clone();
            }
        }

        public double[] this[WeatherDataType weatherDataType]
        {
            get
            {
                return this[weatherDataType.ToString()];
            }
            set
            {
                this[weatherDataType.ToString()] = value;
            }
        }

        public bool Contains(WeatherDataType weatherDataType)
        {
            return Contains(weatherDataType.ToString());
        }

        public bool Contains(string name)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return false;
            }

            return dictionary.ContainsKey(name);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return dictionary == null ? null : dictionary.Keys;
            }
        }

        public IEnumerable<double[]> Values
        {
            get
            {
                return dictionary == null ? null : dictionary.Values;
            }
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if(jObject.ContainsKey("Data"))
            {
                JArray jArray = jObject.Value<JArray>("Data");
                if(jArray != null)
                {
                    dictionary = new Dictionary<string, double[]>();
                    foreach(JObject jObject_Temp in jArray)
                    {
                        if (!jObject_Temp.ContainsKey("Name"))
                            continue;

                        string name = jObject_Temp.Value<string>("Name");
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        double[] values = null;
                        if(jObject_Temp.ContainsKey("Values"))
                        {
                            JArray jArray_Temp = jObject_Temp.Value<JArray>("Values");
                            if (jArray_Temp.Count != 24)
                                continue;

                            values = jArray_Temp.ToList<double>().ToArray();
                        }

                        dictionary[name] = values;
                    }
                }
            }


            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(dictionary != null)
            {
                JArray jArray = new JArray();
                foreach(KeyValuePair<string, double[]> keyValuePair in dictionary)
                {
                    JObject jObject_Temp = new JObject();
                    jObject_Temp.Add("Name", keyValuePair.Key);
                    jObject_Temp.Add("Values", new JArray(keyValuePair.Value));
                    jArray.Add(jObject_Temp);
                }
                jObject.Add("Data", jArray);
            }

            return jObject;
        }

        public double CalculatedGroundTemperature(int index)
        {
            double dryBulbTemperature = this[WeatherDataType.DryBulbTemperature, index];
            if (double.IsNaN(dryBulbTemperature))
                return double.NaN;

            double globalRadiation = CalculatedGlobalRadiation(index);
            if (double.IsNaN(globalRadiation))
                return double.NaN;

            return (0.02 * globalRadiation) + dryBulbTemperature;
        }

        public double CalculatedGlobalRadiation(int index)
        {
            double result = this[WeatherDataType.GlobalSolarRadiation, index];
            if (!double.IsNaN(result))
                return result;

            double directSolarRadiation = this[WeatherDataType.DirectSolarRadiation, index];
            if (double.IsNaN(directSolarRadiation))
                return double.NaN;

            double diffuseSolarRadiation = this[WeatherDataType.DiffuseSolarRadiation, index];
            if (double.IsNaN(diffuseSolarRadiation))
                return double.NaN;

            return directSolarRadiation + diffuseSolarRadiation;
        }
    }
}