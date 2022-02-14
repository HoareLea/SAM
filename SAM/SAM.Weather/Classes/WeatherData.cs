using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Weather
{
    public class WeatherData : SAMObject, IWeatherObject
    {
        private string description;
        private double latitude;
        private double longitude;
        private double elevation;
        private List<WeatherYear> weatherYears;

        public WeatherData()
        {

        }

        public WeatherData(WeatherData weatherData)
            : base(weatherData)
        {
            description = weatherData.description;
            latitude = weatherData.latitude;
            longitude = weatherData.longitude;
            elevation = weatherData.elevation;

            if(weatherData.weatherYears != null)
            {
                weatherYears = new List<WeatherYear>();
                foreach(WeatherYear weatherYear in weatherData.weatherYears)
                    weatherYears.Add(weatherYear == null ? null : new WeatherYear(weatherYear));
            }
        }
        
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

        public WeatherData(string name, string description, double latitude, double longitude, double elevation)
            : base(name)
        {
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;
        }

        public WeatherData(double latitude, double longitude, double elevation)
        : base()
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;
        }

        public WeatherData(JObject jObject)
            : base(jObject)
        {
        }

        public WeatherYear this[int year]
        {
            get
            {
                if (weatherYears == null)
                    return null;

                return weatherYears.Find(x => x.Year == year);
            }
        }

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

        public Location Location
        {
            get
            {
                Location result =  new Location(Name, longitude, latitude, elevation);
                if(TryGetValue(WeatherDataParameter.TimeZone, out string timeZone))
                {
                    result.SetValue(LocationParameter.TimeZone, timeZone);
                }

                return result;
            }
        }

        public IEnumerable<int> Years
        {
            get
            {
                if (weatherYears == null)
                    return null;

                return weatherYears.ConvertAll(x => x.Year);
            }
        }

        public List<WeatherYear> WeatherYears
        {
            get
            {
                return weatherYears.ConvertAll(x => x == null ? null : new WeatherYear(x));
            }
        }

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