using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class WeatherCaseData : BuiltInCaseData
    {
        private string weatherDataName;

        public WeatherCaseData(string weatherDataName)
            : base(nameof(OpeningCaseData))
        {
            this.weatherDataName = weatherDataName;
        }

        public WeatherCaseData(JObject jObject)
            : base(jObject)
        {

        }

        public WeatherCaseData(WeatherCaseData weatherCaseData)
            : base(weatherCaseData)
        {
            if(weatherCaseData != null)
            {
                weatherDataName = weatherCaseData.weatherDataName;
            }
        }

        public string WeatherDataName
        {
            get 
            { 
                return weatherDataName; 
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return false;
            }

            if(jObject.ContainsKey("WeatherDataName"))
            {
                weatherDataName = jObject.Value<string>("WeatherDataName");
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result is null)
            {
                return result;
            }

            if(weatherDataName is not null)
            {
                result.Add("WeatherDataName", weatherDataName);
            }

            return result;
        }
    }
}