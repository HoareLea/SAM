using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Weather
{
    public class WeatherYear : SAMObject
    {
        private string description;
        private double latitude;
        private double longitude;
        private WeatherDay[] weatherDays;

        public WeatherYear(WeatherYear weatherYear)
            : base(weatherYear)
        {
            description = weatherYear.description;
            latitude = weatherYear.latitude;
            longitude = weatherYear.longitude;
        }

        public WeatherYear(string name, string description, double latitude, double longitude)
            : base(name)
        {
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public WeatherYear(JObject jObject)
            : base(jObject)
        {
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

            if(jObject.ContainsKey("WeatherDays"))
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

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (description != null)
                jObject.Add("Description", description);

            if (double.IsNaN(latitude))
                jObject.Add("Latitude", latitude);

            if (double.IsNaN(longitude))
                jObject.Add("Longitude", longitude);

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