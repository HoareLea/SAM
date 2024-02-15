using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    /// <summary>
    /// Represents a single day's weather data.
    /// </summary>
    public class WeatherDay : IWeatherObject
    {
        private Dictionary<string, double[]> dictionary;

        /// <summary>
        /// Constructor for WeatherDay class, initializing a new Dictionary.
        /// </summary>
        /// <returns>
        /// A new instance of WeatherDay.
        /// </returns>
        public WeatherDay()
        {
            dictionary = new Dictionary<string, double[]>();
        }

        /// <summary>
        /// Constructor for WeatherDay class that takes a WeatherDay object as a parameter.
        /// </summary>
        /// <param name="weatherDay">WeatherDay object to be used for initialization.</param>
        /// <returns>A new WeatherDay object.</returns>
        public WeatherDay(WeatherDay weatherDay)
        {
            dictionary = new Dictionary<string, double[]>();
            if (weatherDay != null)
            {
                foreach (KeyValuePair<string, double[]> keyValuePair in weatherDay.dictionary)
                {
                    dictionary[keyValuePair.Key] = (double[])keyValuePair.Value.Clone();
                }
            }
        }

        /// <summary>
        /// Constructor for WeatherDay class that takes a JObject as a parameter.
        /// </summary>
        /// <param name="jObject">JObject to be used to construct the WeatherDay object.</param>
        /// <returns>
        /// WeatherDay object constructed from the given JObject.
        /// </returns>
        public WeatherDay(JObject jObject)
        {
            FromJObject(jObject);
        }

        /// <summary>
        /// Gets or sets the value of the specified index for the given name.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <param name="index">The index of the value.</param>
        /// <returns>The value of the specified index for the given name.</returns>
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

        /// <summary>
        /// Gets or sets the weather data type value at the specified index.
        /// </summary>
        /// <param name="weatherDataType">The weather data type.</param>
        /// <param name="index">The index.</param>
        /// <returns>The weather data type value at the specified index.</returns>
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

        /// <summary>
        /// Gets or sets the double array value associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the double array value to get or set.</param>
        /// <returns>The double array value associated with the specified name.</returns>
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

                if (value == null)
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

        /// <summary>
        /// Gets or sets the array of weather data values for the specified weather data type.
        /// </summary>
        /// <param name="weatherDataType">The type of weather data.</param>
        /// <returns>The array of weather data values.</returns>
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

        /// <summary>
        /// Checks if the WeatherDataType is contained in the collection.
        /// </summary>
        /// <param name="weatherDataType">The WeatherDataType to check for.</param>
        /// <returns>True if the WeatherDataType is contained in the collection, false otherwise.</returns>
        public bool Contains(WeatherDataType weatherDataType)
        {
            return Contains(weatherDataType.ToString());
        }

        /// <summary>
        /// Checks if the dictionary contains the specified name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if the dictionary contains the specified name, false otherwise.</returns>
        public bool Contains(string name)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return false;
            }

            return dictionary.ContainsKey(name);
        }

        /// <summary>
        /// Removes an item from the dictionary.
        /// </summary>
        /// <param name="name">The name of the item to remove.</param>
        /// <returns>True if the item was removed, false otherwise.</returns>
        public bool Remove(string name)
        {
            if (dictionary == null || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (!dictionary.ContainsKey(name))
            {
                return false;
            }

            return dictionary.Remove(name);
        }

        /// <summary>
        /// Removes the specified WeatherDataType from the collection.
        /// </summary>
        /// <param name="weatherDataType">The WeatherDataType to remove.</param>
        /// <returns>True if the WeatherDataType was successfully removed, false otherwise.</returns>
        public bool Remove(WeatherDataType weatherDataType)
        {
            return Remove(weatherDataType.ToString());
        }

        public double Average(string name)
        {
            if (dictionary == null || dictionary.Count == 0 || name == null)
            {
                return double.NaN;
            }

            if(!dictionary.ContainsKey(name))
            {
                return double.NaN;
            }

            double[] values = dictionary[name];
            if(values == null)
            {
                return double.NaN;
            }

            return values.Sum() / System.Convert.ToDouble(values.Length);
        }

        public double Average(WeatherDataType weatherDataType)
        {
            return Average(weatherDataType.ToString());
        }

        /// <summary>
        /// Gets the keys of the dictionary.
        /// </summary>
        /// <returns>The keys of the dictionary.</returns>
        public IEnumerable<string> Keys
        {
            get
            {
                return dictionary == null ? null : dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets the values of the dictionary.
        /// </summary>
        /// <returns>The values of the dictionary.</returns>
        public IEnumerable<double[]> Values
        {
            get
            {
                return dictionary == null ? null : dictionary.Values;
            }
        }

        /// <summary>
        /// Gets the values of the specified weather data type.
        /// </summary>
        /// <param name="weatherDataType">The weather data type.</param>
        /// <returns>A list of double values.</returns>
        public List<double> GetValues(WeatherDataType weatherDataType)
        {
            return GetValues(weatherDataType.ToString());
        }

        public List<double> GetValues(string name)
        {
            return this[name]?.ToList();
        }

        public IndexedDoubles GetIndexedDoubles(string name)
        {
            List<double> values = GetValues(name);
            if (values == null)
            {
                return null;
            }

            return new IndexedDoubles(values);
        }

        public IndexedDoubles GetIndexedDoubles(WeatherDataType weatherDataType)
        {
            return GetIndexedDoubles(weatherDataType.ToString());
        }

        /// <summary>
        /// Deserializes a JObject into a dictionary of string and double array.
        /// </summary>
        /// <param name="jObject">The JObject to deserialize.</param>
        /// <returns>True if the deserialization was successful, false otherwise.</returns>
        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Data"))
            {
                JArray jArray = jObject.Value<JArray>("Data");
                if (jArray != null)
                {
                    dictionary = new Dictionary<string, double[]>();
                    foreach (JObject jObject_Temp in jArray)
                    {
                        if (!jObject_Temp.ContainsKey("Name"))
                            continue;

                        string name = jObject_Temp.Value<string>("Name");
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        double[] values = null;
                        if (jObject_Temp.ContainsKey("Values"))
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

        /// <summary>
        /// Converts the object to a JObject.
        /// </summary>
        /// <returns>A JObject representing the object.</returns>
        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (dictionary != null)
            {
                JArray jArray = new JArray();
                foreach (KeyValuePair<string, double[]> keyValuePair in dictionary)
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

        /// <summary>
        /// Calculates the ground temperature based on the dry bulb temperature and global radiation.
        /// </summary>
        /// <param name="index">The index of the weather data.</param>
        /// <returns>The calculated ground temperature.</returns>
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

        /// <summary>
        /// Calculates the global solar radiation from direct and diffuse solar radiation.
        /// </summary>
        /// <param name="index">The index of the data.</param>
        /// <returns>
        /// The global solar radiation.
        /// </returns>
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

        public WeatherHour GetWeatherHour(int index)
        {
            if (index < 0 || index > 23)
            {
                return null;
            }

            WeatherHour result = new WeatherHour();
            foreach(KeyValuePair<string, double[]> keyValuePair in dictionary)
            {
                result[keyValuePair.Key] = keyValuePair.Value[index];
            }

            return result;
        }

        public List<WeatherHour> GetWeatherHours()
        {
            List<WeatherHour> result = new List<WeatherHour>();
            for(int i=0; i < 24; i++)
            {
                result.Add(GetWeatherHour(i));
            }

            return result;
        }

        public List<WeatherHour> GetWeatherHours(IEnumerable<int> hours)
        {
            if(hours == null)
            {
                return null;
            }

            List<WeatherHour> result = new List<WeatherHour>();
            foreach(int hour in hours)
            {
                result.Add(GetWeatherHour(hour));
            }

            return result;
        }
    }
}