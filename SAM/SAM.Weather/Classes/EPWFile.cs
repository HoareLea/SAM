using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public class EPWFile : IJSAMObject
    {
        private string country;
        private string state;
        private string city;
        private string dataSource;
        private string wMONumber;
        private int timeZone;
        private int year;
        private string comments_1;
        private string comments_2;
        private WeatherYear weatherYear;

        public EPWFile()
        { 
            
        }
        
        public EPWFile(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Country
        {
            get
            {
                return country;
            }
            set
            {
                country = value;
            }
        }

        public string State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        public string City
        {
            get
            {
                return city;
            }
            set
            {
                city = value;
            }
        }

        public string DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
            }
        }

        public string WMONumber
        {
            get
            {
                return wMONumber;
            }
            set
            {
                wMONumber = value;
            }
        }

        public int TimeZone
        {
            get
            {
                return timeZone;
            }
            set
            {
                timeZone = value;
            }
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

        public string Comments_1
        {
            get
            {
                return comments_1;
            }
            set
            {
                comments_1 = value;
            }
        }

        public string Comments_2
        {
            get
            {
                return comments_2;
            }
            set
            {
                comments_2 = value;
            }
        }

        public double Longitude
        {
            get
            {
                if (weatherYear == null)
                    return default;

                return weatherYear.Longitude;
            }
            set
            {
                if (weatherYear != null)
                    weatherYear.Longitude = value;
            }
        }

        public double Latitude
        {
            get
            {
                if (weatherYear == null)
                    return default;

                return weatherYear.Latitude;
            }
            set
            {
                if (weatherYear != null)
                    weatherYear.Latitude = value;
            }
        }

        public double Elevtion
        {
            get
            {
                if (weatherYear == null)
                    return default;

                return weatherYear.Elevtion;
            }
            set
            {
                if (weatherYear != null)
                    weatherYear.Elevtion = value;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Country"))
                country = jObject.Value<string>("Country");

            if (jObject.ContainsKey("State"))
                state = jObject.Value<string>("State");

            if (jObject.ContainsKey("City"))
                city = jObject.Value<string>("City");

            if (jObject.ContainsKey("DataSource"))
                dataSource = jObject.Value<string>("DataSource");

            if (jObject.ContainsKey("WMONumber"))
                wMONumber = jObject.Value<string>("WMONumber");

            if (jObject.ContainsKey("TimeZone"))
                timeZone = jObject.Value<int>("TimeZone");

            if (jObject.ContainsKey("Year"))
                year = jObject.Value<int>("Year");

            if (jObject.ContainsKey("Comments_1"))
                comments_1 = jObject.Value<string>("Comments_1");

            if (jObject.ContainsKey("Comments_2"))
                comments_2 = jObject.Value<string>("Comments_2");

            if (jObject.ContainsKey("WeatherYear"))
            {
                JObject jObject_WeatherYear = jObject.Value<JObject>("WeatherYear");
                if(jObject_WeatherYear != null)
                    weatherYear = new WeatherYear(jObject_WeatherYear);
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            if (country != null)
                jObject.Add("Country", country);

            if (state != null)
                jObject.Add("State", state);

            if (city != null)
                jObject.Add("City", city);

            if (dataSource != null)
                jObject.Add("DataSource", dataSource);
            
            if(wMONumber != null)
                jObject.Add("WMONumber", wMONumber);

            jObject.Add("TimeZone", timeZone);

            jObject.Add("Year", year);

            if (comments_1 != null)
                jObject.Add("Comments_1", comments_1);

            if (comments_2 != null)
                jObject.Add("Comments_2", comments_2);

            if (weatherYear != null)
                jObject.Add("WeatherYear", weatherYear.ToJObject());

            return jObject;
        }
    }
}