using SAM.Core;
using SAM.Weather;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void UpdateWeather(this AnalyticalModel analyticalModel, WeatherData weatherData = null, IEnumerable<DesignDay> coolingDesignDays = null, IEnumerable<DesignDay> heatingDesignDays = null)
        {
            if(analyticalModel == null)
            {
                return;
            }

            if (weatherData != null)
            {
                analyticalModel.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData(weatherData));
            }
            else
            {
                analyticalModel.RemoveValue(AnalyticalModelParameter.WeatherData);
            }

            if (coolingDesignDays != null)
            {
                analyticalModel.SetValue(AnalyticalModelParameter.CoolingDesignDays, new SAMCollection<DesignDay>(coolingDesignDays));
            }
            else
            {
                analyticalModel.RemoveValue(AnalyticalModelParameter.CoolingDesignDays);
            }

            if (heatingDesignDays != null)
            {
                analyticalModel.SetValue(AnalyticalModelParameter.HeatingDesignDays, new SAMCollection<DesignDay>(heatingDesignDays));
            }
            else
            {
                analyticalModel.RemoveValue(AnalyticalModelParameter.HeatingDesignDays);
            }
        }
    }
}