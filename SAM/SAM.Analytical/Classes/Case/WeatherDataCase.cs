// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Weather;

namespace SAM.Analytical.Classes
{
    public class WeatherDataCase : Case
    {
        private WeatherData weatherData;

        public WeatherDataCase(WeatherData weatherData)
            : base()
        {
            this.weatherData = weatherData;
        }

        public WeatherDataCase(WeatherDataCase weatherDataCase)
            : base(weatherDataCase)
        {
            if (weatherDataCase != null)
            {
                weatherData = weatherDataCase.weatherData;
            }
        }

        public WeatherDataCase(JObject jObject)
            : base(jObject)
        {

        }

        public WeatherData WeatherData
        {
            get
            {
                return weatherData;
            }

            set
            {
                weatherData = value;
                OnPropertyChanged(nameof(WeatherData));
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if (!result)
            {
                return false;
            }

            if (jObject.ContainsKey("WeatherData"))
            {
                weatherData = Core.Query.IJSAMObject<WeatherData>(jObject.Value<JObject>("WeatherData"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result is null)
            {
                return result;
            }

            if (weatherData != null)
            {
                result.Add("WeatherData", weatherData.ToJObject());
            }

            return result;
        }
    }
}
