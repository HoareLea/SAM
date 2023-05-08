namespace SAM.Weather
{
    public static partial class Query
    {
        /// <summary>
        /// Generates a string of design conditions from the given <see cref="WeatherData"/> object.
        /// </summary>
        /// <param name="weatherData">The <see cref="WeatherData"/> object to generate the design conditions string from.</param>
        /// <returns>A string of design conditions.</returns>
        public static string DesignConditionsString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            string[] values = new string[] {
        "DESIGN CONDITIONS",
        (1).ToString(),
        "Climate Design Data 2009 ASHRAE Handbook",
        "",
        "Heating,7,4.3,5.8,-2.2,3.1,14.2,-0.2,3.7,13.4,11.9,16.2,10.5,16.3,1.5,310",
        "Cooling,2,6.3,29.9,22.1,28.2,22.1,27.2,21.8,24.1,27.5,23.5,26.7,22.8,26,6,40,23.1,17.8,26.2,22.4,17.1,25.5,21.7,16.4,24.9,72.7,27.6,70.1,26.9,67.7,26.1,1387",
        "Extremes,11.1,9.9,8.8,29.4,1.6,35.3,1.1,3.2,0.9,37.7,0.2,39.6,-0.4,41.4,-1.1,43.7"
    };

            // Join the values into a single string, separated by commas
            return string.Join(",", values);
        }
    }
}