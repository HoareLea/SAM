using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Weather
{
    /// <summary>
    /// This class represents the WeatherData object which implements the SAMObject and IWeatherObject interfaces. 
    /// </summary>
    public class WeatherData : SAMObject, IWeatherObject
    {
        private string description;
        private double latitude;
        private double longitude;
        private double elevation;
        private List<WeatherYear> weatherYears;

        /// <summary>
        /// Constructor for the WeatherData class.
        /// </summary>
        /// <returns>
        /// Nothing.
        /// </returns>
        public WeatherData()
        {

        }

        /// <summary>
        /// Constructor for WeatherData class that takes a WeatherData object as a parameter and copies its values.
        /// </summary>
        /// <param name="weatherData">WeatherData object to copy values from.</param>
        /// <returns>A new WeatherData object with the same values as the parameter.</returns>
        public WeatherData(WeatherData weatherData)
            : base(weatherData)
        {
            description = weatherData.description;
            latitude = weatherData.latitude;
            longitude = weatherData.longitude;
            elevation = weatherData.elevation;

            if (weatherData.weatherYears != null)
            {
                weatherYears = new List<WeatherYear>();
                foreach (WeatherYear weatherYear in weatherData.weatherYears)
                    weatherYears.Add(weatherYear == null ? null : new WeatherYear(weatherYear));
            }
        }

        public WeatherData(WeatherData weatherData, string name, string description, double latitude, double longitude, double elevation, WeatherYear weatherYear)
            : this(weatherData)
        {
            this.name = name;
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;

            weatherYears = new List<WeatherYear>();
            if(weatherYear != null)
            {
                weatherYears.Add(weatherYear.Clone());
            }
        }

        /// <summary>
        /// Constructor for WeatherData class.
        /// </summary>
        /// <param name="name">Name of the weather data.</param>
        /// <param name="description">Description of the weather data.</param>
        /// <param name="latitude">Latitude of the weather data.</param>
        /// <param name="longitude">Longitude of the weather data.</param>
        /// <param name="elevation">Elevation of the weather data.</param>
        /// <param name="weatherYear">Weather year of the weather data.</param>
        /// <returns>WeatherData object.</returns>
        public WeatherData(string name, string description, double latitude, double longitude, double elevation, WeatherYear weatherYear)
            : base(name)
        {
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;

            weatherYears = new List<WeatherYear>();
            if (weatherYear != null)
                weatherYears.Add(new WeatherYear(weatherYear));
        }

        /// <summary>
        /// Constructor for WeatherData class.
        /// </summary>
        /// <param name="name">Name of the weather data.</param>
        /// <param name="description">Description of the weather data.</param>
        /// <param name="latitude">Latitude of the weather data.</param>
        /// <param name="longitude">Longitude of the weather data.</param>
        /// <param name="elevation">Elevation of the weather data.</param>
        /// <returns>
        /// WeatherData object.
        /// </returns>
        public WeatherData(string name, string description, double latitude, double longitude, double elevation)
            : base(name)
        {
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;
        }

        /// <summary>
        /// Constructor for WeatherData class that sets the latitude, longitude, and elevation of the location.
        /// </summary>
        /// <param name="latitude">The latitude of the location.</param>
        /// <param name="longitude">The longitude of the location.</param>
        /// <param name="elevation">The elevation of the location.</param>
        /// <returns>
        /// A WeatherData object with the specified latitude, longitude, and elevation.
        /// </returns>
        public WeatherData(double latitude, double longitude, double elevation)
        : base()
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;
        }

        /// <summary>
        /// Constructor for WeatherData class that takes a JObject as a parameter.
        /// </summary>
        /// <param name="jObject">JObject to be used for constructing the WeatherData object.</param>
        /// <returns>
        /// WeatherData object constructed from the given JObject.
        /// </returns>
        public WeatherData(JObject jObject)
            : base(jObject)
        {
        }

        /// <summary>
        /// Gets the WeatherYear object for the specified year.
        /// </summary>
        /// <param name="year">The year to get the WeatherYear object for.</param>
        /// <returns>The WeatherYear object for the specified year.</returns>
        public WeatherYear this[int year]
        {
            get
            {
                if (weatherYears == null)
                    return null;

                return weatherYears.Find(x => x.Year == year);
            }
        }

        public WeatherHour this[DateTime dateTime]
        {
            get
            {
                return GetWeatherHour(dateTime);
            }
        }

        public WeatherHour GetWeatherHour(DateTime dateTime)
        {
            WeatherYear weatherYear = this[dateTime.Year];
            if(weatherYear == null)
            {
                return null;
            }

            return weatherYear.GetWeatherHour(dateTime.DayOfYear, dateTime.Hour);
        }

        /// <summary>
        /// Adds a WeatherYear object to the list of WeatherYears.
        /// </summary>
        /// <param name="weatherYear">The WeatherYear object to add.</param>
        /// <returns>True if the WeatherYear was added, false otherwise.</returns>
        public bool Add(WeatherYear weatherYear)
        {
            if (weatherYear == null)
                return false;

            if (weatherYears == null)
                weatherYears = new List<WeatherYear>();

            int index = weatherYears.FindIndex(x => x.Year == weatherYear.Year);
            if (index == -1)
                weatherYears.Add(weatherYear);
            else
                weatherYears[index] = weatherYear;

            return true;
        }

        /// <summary>
        /// Adds a new set of weather values to the collection for the specified date and time.
        /// </summary>
        /// <param name="dateTime">The date and time to add the values for.</param>
        /// <param name="values">The values to add.</param>
        /// <returns>True if the values were added successfully, false otherwise.</returns>
        public bool Add(DateTime dateTime, Dictionary<string, double> values)
        {
            if (dateTime == DateTime.MinValue)
                return false;

            if (weatherYears == null)
                weatherYears = new List<WeatherYear>();

            WeatherYear weatherYear = weatherYears.Find(x => x.Year == dateTime.Year);
            if (weatherYear == null)
            {
                weatherYear = new WeatherYear(dateTime.Year);
                weatherYears.Add(weatherYear);
            }

            int day = dateTime.DayOfYear - 1;
            int hour = dateTime.Hour;

            return weatherYear.Add(day, hour, values);
        }

        /// <summary>
        /// Removes a weather year from the list of weather years.
        /// </summary>
        /// <param name="year">The year to remove.</param>
        /// <returns>True if the year was removed, false otherwise.</returns>
        public bool Remove(int year)
        {
            if (weatherYears == null)
                return false;

            int index = weatherYears.FindIndex(x => x.Year == year);
            if (index == -1)
                return false;

            weatherYears.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Longitude [Degrees] minimum -180, maximum +180, - is West, + is East, degree minutes represented in decimal (i.e. 30 minutes is .5)
        /// </summary>
        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                longitude = value;
            }
        }

        /// <summary>
        /// Latitude [Degrees] minimum -90, maximum +90, + is North, - is South, degree minutes represented in decimal (i.e. 30 minutes is .5)
        /// </summary>
        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
            }
        }

        /// <summary>
        /// Elevation [m], minimum -1000.0, maximum  +9999.9
        /// </summary>
        public double Elevtion
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the object.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// Gets the Location object with the given Name, longitude, latitude and elevation. 
        /// If the TimeZone is available, it is also set in the Location object. 
        /// </summary>
        /// <returns>Location object with the given Name, longitude, latitude and elevation.</returns>
        public Location Location
        {
            get
            {
                Location result = new Location(Name, longitude, latitude, elevation);
                if (TryGetValue(WeatherDataParameter.TimeZone, out string timeZone))
                {
                    result.SetValue(LocationParameter.TimeZone, timeZone);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the years of the weather data.
        /// </summary>
        /// <returns>A collection of years.</returns>
        public IEnumerable<int> Years
        {
            get
            {
                if (weatherYears == null)
                    return null;

                return weatherYears.ConvertAll(x => x.Year);
            }
        }

        public IndexedDoubles GetIndexedDoubles(string name)
        {
            if(this.weatherYears == null)
            {
                return null;
            }

            IndexedDoubles result = new IndexedDoubles();

            List<WeatherYear> weatherYears = new List<WeatherYear>(this.weatherYears);
            weatherYears.RemoveAll(x => x == null);
            weatherYears.Sort((x, y) => x.Year.CompareTo(y.Year));

            if(weatherYears == null || weatherYears.Count == 0)
            {
                return result;
            }

            DateTime dateTime = new DateTime(weatherYears[0].Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day);

            foreach (WeatherYear weatherYear in weatherYears)
            {
                DateTime dateTime_Temp = new DateTime(weatherYear.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day);

                int index = (int)(dateTime_Temp - dateTime).TotalHours;

                IndexedDoubles indexedDoubles = weatherYear.GetIndexedDoubles(name);
                foreach(int key in indexedDoubles.Keys)
                {
                    result[index + key] = indexedDoubles[key];
                }
            }

            return result;
        }

        public IndexedDoubles GetIndexedDoubles(WeatherDataType weatherDataType)
        {
            return GetIndexedDoubles(weatherDataType.ToString());
        }

        /// <summary>
        /// Gets a list of WeatherYear objects.
        /// </summary>
        /// <returns>
        /// A list of WeatherYear objects.
        /// </returns>
        public List<WeatherYear> WeatherYears
        {
            get
            {
                return weatherYears.ConvertAll(x => x == null ? null : new WeatherYear(x));
            }
        }

        /// <summary>
        /// Deserializes a JObject into a WeatherStation object.
        /// </summary>
        /// <param name="jObject">The JObject to deserialize.</param>
        /// <returns>True if the deserialization was successful, false otherwise.</returns>
        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Description"))
                description = jObject.Value<string>("Description");

            if (jObject.ContainsKey("Latitude"))
                latitude = jObject.Value<double>("Latitude");

            if (jObject.ContainsKey("Longitude"))
                longitude = jObject.Value<double>("Longitude");

            if (jObject.ContainsKey("Elevation"))
                elevation = jObject.Value<double>("Elevation");

            if (jObject.ContainsKey("WeatherYears"))
            {
                JArray jArray = jObject.Value<JArray>("WeatherYears");
                if (jArray != null)
                {
                    weatherYears = new List<WeatherYear>();
                    foreach (JObject jObject_Temp in jArray)
                    {
                        if (jObject_Temp == null)
                            continue;

                        weatherYears.Add(new WeatherYear(jObject_Temp));
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Converts the object to a JSON object.
        /// </summary>
        /// <returns>A JSON object representing the object.</returns>
        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (description != null)
                jObject.Add("Description", description);

            if (!double.IsNaN(latitude))
                jObject.Add("Latitude", latitude);

            if (!double.IsNaN(longitude))
                jObject.Add("Longitude", longitude);

            if (!double.IsNaN(elevation))
                jObject.Add("Elevation", elevation);

            if (weatherYears != null)
            {
                JArray jArray = new JArray();
                foreach (WeatherYear weatherYear in weatherYears)
                {
                    JObject jObject_Temp = weatherYear?.ToJObject();
                    if (jObject_Temp != null)
                        jArray.Add(jObject_Temp);
                }
                jObject.Add("WeatherYears", jArray);
            }

            return jObject;
        }
    }
}