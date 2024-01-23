using SAM.Core;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static Range<double> DryBulbTemperatureRange(double dryBulbTemperature)
        {
            if(double.IsNaN(dryBulbTemperature))
            {
                return null;
            }

            double upperLimit = double.NaN;
            double lowerLimit = double.NaN;

            if (dryBulbTemperature >= 10 && dryBulbTemperature <= 33.5)
            {
                upperLimit = (dryBulbTemperature * 0.31) + 17.8 + 3.5;
                lowerLimit = (dryBulbTemperature * 0.31) + 17.8 - 3.5;
            }
            else if( dryBulbTemperature < 10 )
            {
                lowerLimit = (10 * 0.31) + 17.8 - 3.5;
                upperLimit = (10 * 0.31) + 17.8 + 3.5;
            }
            else if( dryBulbTemperature > 33.5)
            {
                upperLimit = (33.5 * 0.31) + 17.8 + 3.5;
                lowerLimit = (33.5 * 0.31) + 17.8 - 3.5;
            }

            if(double.IsNaN(lowerLimit) || double.IsNaN(upperLimit))
            {
                return null;
            }

            return new Range<double>(lowerLimit, upperLimit);
        }

        public static Range<double> DryBulbTemperatureRange(this WeatherHour weatherHour)
        {
            if(weatherHour == null)
            {
                return null;
            }

            double dryBulbTemperature = weatherHour[WeatherDataType.DryBulbTemperature];
            if(double.IsNaN(dryBulbTemperature))
            {
                return null;
            }

            return DryBulbTemperatureRange(dryBulbTemperature);
        }
    }
}