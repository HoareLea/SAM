using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public class WeatherYear : IJSAMObject
    {
        private int year;
        private WeatherDay[] weatherDays;

        public WeatherYear(int year)
        {
            this.year = year;
        }

        public WeatherYear(WeatherYear weatherYear)
        {
            year = weatherYear.year;
            
            if(weatherYear.weatherDays != null)
            {
                weatherDays = new WeatherDay[weatherYear.weatherDays.Length];
                for (int i = 0; i < weatherYear.weatherDays.Length; i++)
                    weatherDays[i] = weatherYear.weatherDays[i] == null ? null : new WeatherDay(weatherYear.weatherDays[i]);
            }
        }

        public WeatherYear(JObject jObject)
        {
            FromJObject(jObject);
        }

        public WeatherDay this[int i]
        {
            get
            {
                if (i < 0 || i >= 365)
                    return null;

                if (weatherDays == null)
                    return null;

                return weatherDays[i];
            }
            set
            {
                if (i < 0 || i >= 365)
                    return;

                if (weatherDays == null)
                    weatherDays = new WeatherDay[365];

                weatherDays[i] = value;
            }
        }

        public bool Add(int day, int hour, Dictionary<string, double> values)
        {
            if (day < 0 || day >= 365)
                return false;

            if (hour < 0 || hour >= 24)
                return false;

            if (weatherDays == null)
                weatherDays = new WeatherDay[365];

            WeatherDay weatherDay = weatherDays[day];
            if(weatherDay == null)
            {
                weatherDay = new WeatherDay();
                weatherDays[day] = weatherDay;
            }
            
            foreach(KeyValuePair<string, double> keyValuePair in values)
                weatherDay[keyValuePair.Key, hour] = keyValuePair.Value;

            return true;
        }

        public int Year
        {
            get
            {
                return year;
            }
            set
            {
                year = value;
            }
        }

        public List<WeatherDay> WeatherDays
        {
            get
            {
                if (weatherDays == null)
                    return null;

                return weatherDays.ToList();
            }
        }

        public List<double> GetValues(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            List<double> result = new List<double>();
            for(int i=0; i < weatherDays.Length; i++)
            {
                WeatherDay weatherDay = weatherDays[i];
                if(weatherDay == null)
                {
                    result.AddRange(Enumerable.Repeat(double.NaN, 24));
                    continue;
                }

                double[] values = weatherDay[name];
                if (values == null || values.Length != 24)
                    result.AddRange(Enumerable.Repeat(double.NaN, 24));
            }

            return result;
        }

        public List<double> GetValues(WeatherDataType weatherDataType)
        {
            return GetValues(weatherDataType.ToString());
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Year"))
                year = jObject.Value<int>("Year");

            if (jObject.ContainsKey("WeatherDays"))
            {
                JArray jArray = jObject.Value<JArray>("WeatherDays");
                if(jArray != null && jArray.Count == 365)
                {
                    weatherDays = new WeatherDay[365];
                    for (int i = 0; i < 365; i++)
                    {
                        JObject jObject_Temp = jArray[i] as JObject;
                        if (jObject_Temp == null)
                            continue;

                        weatherDays[i] =new WeatherDay(jObject_Temp);
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (year != int.MinValue)
                jObject.Add("Year", year);

            if(weatherDays != null)
            {
                JArray jArray = new JArray();
                foreach (WeatherDay weatherDay in weatherDays)
                    jArray.Add(weatherDay?.ToJObject());

                jObject.Add("WeatherDays", jArray);
            }

            return jObject;
        }
    }
}