using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Weather
{
    /// <summary>
    /// Represents a single hour's weather data.
    /// </summary>
    public class WeatherHour : IWeatherObject
    {
        private Dictionary<string, double> dictionary;

        /// <summary>
        /// Constructor for WeatherDay class, initializing a new Dictionary.
        /// </summary>
        /// <returns>
        /// A new instance of WeatherDay.
        /// </returns>
        public WeatherHour()
        {
            dictionary = new Dictionary<string, double>();
        }

        /// <summary>
        /// Constructor for WeatherHour class that takes a WeatherHour object as a parameter.
        /// </summary>
        /// <param name="weatherHour">WeatherHour object to be used for initialization.</param>
        /// <returns>A new WeatherHour object.</returns>
        public WeatherHour(WeatherHour weatherHour)
        {
            dictionary = new Dictionary<string, double>();
            if (weatherHour != null)
            {
                foreach (KeyValuePair<string, double> keyValuePair in weatherHour.dictionary)
                {
                    dictionary[keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        /// <summary>
        /// Constructor for WeatherHour class that takes a JObject as a parameter.
        /// </summary>
        /// <param name="jObject">JObject to be used to construct the WeatherHour object.</param>
        /// <returns>
        /// WeatherHour object constructed from the given JObject.
        /// </returns>
        public WeatherHour(JObject jObject)
        {
            FromJObject(jObject);
        }

        /// <summary>
        /// Gets or sets the value for the given name.
        /// </summary>
        /// <param name="name">The name of the value.</param>
        /// <returns>The value for the given name.</returns>
        public double this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name) || dictionary == null)
                {
                    return double.NaN;
                }

                if (!dictionary.TryGetValue(name, out double value))
                {
                    return double.NaN;
                }

                return value;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }

                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, double>();
                }

                dictionary[name] = value;
            }
        }

        /// <summary>
        /// Gets or sets the weather data type value.
        /// </summary>
        /// <param name="weatherDataType">The weather data type.</param>
        /// <returns>The weather data type value.</returns>
        public double this[WeatherDataType weatherDataType]
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
                    dictionary = new Dictionary<string, double>();
                    foreach (JObject jObject_Temp in jArray)
                    {
                        if (!jObject_Temp.ContainsKey("Name"))
                            continue;

                        string name = jObject_Temp.Value<string>("Name");
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        double value = double.NaN;
                        if (jObject_Temp.ContainsKey("Value"))
                        {
                            value = jObject_Temp.Value<double>("Value");
                        }

                        dictionary[name] = value;
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
                foreach (KeyValuePair<string, double> keyValuePair in dictionary)
                {
                    JObject jObject_Temp = new JObject();
                    jObject_Temp.Add("Name", keyValuePair.Key);
                    jObject_Temp.Add("Value", keyValuePair.Value);
                    jArray.Add(jObject_Temp);
                }
                jObject.Add("Data", jArray);
            }

            return jObject;
        }

        /// <summary>
        /// Calculates the ground temperature based on the dry bulb temperature and global radiation.
        /// </summary>
        /// <returns>The calculated ground temperature.</returns>
        public double CalculatedGroundTemperature()
        {
            double dryBulbTemperature = this[WeatherDataType.DryBulbTemperature];
            if (double.IsNaN(dryBulbTemperature))
            {
                return double.NaN;
            }

            double globalRadiation = CalculatedGlobalRadiation();
            if (double.IsNaN(globalRadiation))
            {
                return double.NaN;
            }

            return (0.02 * globalRadiation) + dryBulbTemperature;
        }

        /// <summary>
        /// Calculates the global solar radiation from direct and diffuse solar radiation.
        /// </summary>
        /// <returns>
        /// The global solar radiation.
        /// </returns>
        public double CalculatedGlobalRadiation()
        {
            double result = this[WeatherDataType.GlobalSolarRadiation];
            if (!double.IsNaN(result))
            {
                return result;
            }

            double directSolarRadiation = this[WeatherDataType.DirectSolarRadiation];
            if (double.IsNaN(directSolarRadiation))
            {
                return double.NaN;
            }

            double diffuseSolarRadiation = this[WeatherDataType.DiffuseSolarRadiation];
            if (double.IsNaN(diffuseSolarRadiation))
            {
                return double.NaN;
            }

            return directSolarRadiation + diffuseSolarRadiation;
        }

        public double GlobalSolarRadiation
        {
            get
            {
                return this[WeatherDataType.GlobalSolarRadiation];
            }
            set
            {
                this[WeatherDataType.GlobalSolarRadiation] = value;
            }
        }

        public double DiffuseSolarRadiation
        {
            get
            {
                return this[WeatherDataType.DiffuseSolarRadiation];
            }
            set
            {
                this[WeatherDataType.DiffuseSolarRadiation] = value;
            }
        }

        public double DirectSolarRadiation
        {
            get
            {
                return this[WeatherDataType.DirectSolarRadiation];
            }
            set
            {
                this[WeatherDataType.DirectSolarRadiation] = value;
            }
        }

        public double CloudCover
        {
            get
            {
                return this[WeatherDataType.CloudCover];
            }
            set
            {
                this[WeatherDataType.CloudCover] = value;
            }
        }

        public double DryBulbTemperature
        {
            get
            {
                return this[WeatherDataType.DryBulbTemperature];
            }
            set
            {
                this[WeatherDataType.DryBulbTemperature] = value;
            }
        }

        public double WetBulbTemperature
        {
            get
            {
                return this[WeatherDataType.WetBulbTemperature];
            }
            set
            {
                this[WeatherDataType.WetBulbTemperature] = value;
            }
        }

        public double RelativeHumidity
        {
            get
            {
                return this[WeatherDataType.RelativeHumidity];
            }
            set
            {
                this[WeatherDataType.RelativeHumidity] = value;
            }
        }

        public double WindSpeed
        {
            get
            {
                return this[WeatherDataType.WindSpeed];
            }
            set
            {
                this[WeatherDataType.WindSpeed] = value;
            }
        }

        public double WindDirection
        {
            get
            {
                return this[WeatherDataType.WindDirection];
            }
            set
            {
                this[WeatherDataType.WindDirection] = value;
            }
        }

        public double AtmosphericPressure
        {
            get
            {
                return this[WeatherDataType.AtmosphericPressure];
            }
            set
            {
                this[WeatherDataType.AtmosphericPressure] = value;
            }
        }
    }
}