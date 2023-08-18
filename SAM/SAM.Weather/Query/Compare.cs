using SAM.Core;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static bool Compare(this WeatherHour weatherHour, WeatherDataType weatherDataType, double value, NumberComparisonType numberComparisonType)
        {
            if(weatherHour == null || weatherDataType == WeatherDataType.Undefined)
            {
                return false;
            }

            double value_WeatherHour = weatherHour[weatherDataType];
            if(double.IsNaN(value_WeatherHour))
            {
                return false;
            }

            return Core.Query.Compare(value_WeatherHour, value, numberComparisonType);
        }
    }
}
