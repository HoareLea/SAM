using System.Linq;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static double MaxValue(this WeatherDay weatherDay, WeatherDataType weatherDataType)
        {
            if(weatherDay == null)
            {
                return double.NaN;
            }

            if(!weatherDay.Contains(weatherDataType))
            {
                return double.NaN;
            }

            return weatherDay.GetValues(weatherDataType).Max();
        }
    }
}