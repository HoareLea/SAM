namespace SAM.Weather
{
    public static partial class Query
    {
        public static string LocationString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            double latitude = weatherData.Latitude;
            double longitude = weatherData.Longitude;
            double elevation = weatherData.Elevtion;

            string city = null;
            weatherData.TryGetValue(WeatherDataParameter.City, out city);

            string state = null;
            weatherData.TryGetValue(WeatherDataParameter.State, out state);

            string country = null;
            weatherData.TryGetValue(WeatherDataParameter.Country, out country);

            string dataSource = null;
            weatherData.TryGetValue(WeatherDataParameter.DataSource, out dataSource);

            string wMONumber = null;
            weatherData.TryGetValue(WeatherDataParameter.WMONumber, out wMONumber);

            string timeZone = null;
            weatherData.TryGetValue(WeatherDataParameter.TimeZone, out timeZone);

            string[] values = new string[]
                {
                "LOCATION",
                city == null ? string.Empty : city,
                state == null ? string.Empty : state,
                country == null ? string.Empty : country,
                dataSource == null ? string.Empty : dataSource,
                wMONumber == null ? string.Empty : wMONumber,
                double.IsNaN(latitude) ? string.Empty : latitude.ToString(),
                double.IsNaN(longitude) ? string.Empty : longitude.ToString(),
                string.IsNullOrWhiteSpace(timeZone) ? string.Empty : Core.Query.Double(Core.Query.UTC(timeZone)).ToString(),
                double.IsNaN(elevation) ? string.Empty : elevation.ToString(),
                string.Empty
                };

            return string.Join(",", values);
        }
    }
}