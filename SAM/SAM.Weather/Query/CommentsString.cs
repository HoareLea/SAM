namespace SAM.Weather
{
    /// <summary>
    /// This class provides methods for creating and executing queries against a database.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Generates a string of comments from the given WeatherData object.
        /// </summary>
        /// <param name="weatherData">The WeatherData object to generate the comments string from.</param>
        /// <returns>A string of comments from the given WeatherData object.</returns>
        public static string CommentsString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            string comments_1 = null;
            weatherData.TryGetValue(WeatherDataParameter.Comments_1, out comments_1);

            string comments_2 = null;
            weatherData.TryGetValue(WeatherDataParameter.Comments_2, out comments_2);

            string[] values_1 = new string[] {
                "COMMENTS 1",
                comments_1 == null ? string.Empty : comments_1
            };

            string[] values_2 = new string[] {
                "COMMENTS 2",
                comments_2 == null ? string.Empty : comments_2
            };

            return string.Join("\n", new string[] { string.Join(",", values_1), string.Join(",", values_2) });
        }
    }
}