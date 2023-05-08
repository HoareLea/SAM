namespace SAM.Weather
{
    /// <summary>
    /// A static partial class that contains methods for querying weather data.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Tries to get the value associated with the specified name from the given text.
        /// </summary>
        /// <param name="text">The text containing the value to be retrieved.</param>
        /// <param name="name">The name associated with the value.</param>
        /// <param name="value">The retrieved value, if successful.</param>
        /// <returns>True if the value is retrieved successfully, false otherwise.</returns>
        public static bool TryGetValue(string text, string name, out string value)
        {
            value = null;

            // Check if the input text is empty or consists only of white-space characters.
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Remove leading and trailing white-space characters from the input name.
            string name_Temp = name.Trim();

            // Remove leading white-space characters from the input text.
            string result = text.TrimStart();

            // Check if the input text starts with the input name, case-insensitive.
            if (!result.ToUpper().StartsWith(name_Temp.ToUpper()))
                return false;

            // Remove the input name from the input text.
            result = result.Substring(name_Temp.Length);

            // Remove leading white-space characters.
            result = result.TrimStart();

            // If there's a comma after the input name, remove it along with any leading white-space characters.
            if (result.StartsWith(","))
                result = result.Substring(1).TrimStart();

            value = result;
            return true;
        }

        /// <summary>
        /// Tries to get the value associated with the specified WeatherDataType from the given WeatherDay object at the specified index.
        /// </summary>
        /// <param name="weatherDay">The WeatherDay object containing the weather data.</param>
        /// <param name="weatherDataType">The WeatherDataType to search for.</param>
        /// <param name="index">The index at which to retrieve the value.</param>
        /// <param name="value">The retrieved value, if successful.</param>
        /// <returns>True if the value is retrieved successfully, false otherwise.</returns>
        public static bool TryGetValue(this WeatherDay weatherDay, WeatherDataType weatherDataType, int index, out double value)
        {
            
            value = default;
            if (weatherDay == null || weatherDataType == WeatherDataType.Undefined)
            {
                return false;
            }

            return TryGetValue(weatherDay, weatherDataType.ToString(), index, out value);
        }

        /// <summary>
        /// Tries to get the value associated with the specified name from the given WeatherDay object at the specified index.
        /// </summary>
        /// <param name="weatherDay">The WeatherDay object containing the weather data.</param>
        /// <param name="name">The name associated with the value.</param>
        /// <param name="index">The index at which to retrieve the value.</param>
        /// <param name="value">The retrieved value, if successful.</param>
        /// <returns>True if the value is retrieved successfully, false otherwise.</returns>
        public static bool TryGetValue(this WeatherDay weatherDay, string name, int index, out double value)
        {
            value = default;
            if (weatherDay == null || string.IsNullOrEmpty(name) || index == -1)
            {
                return false;
            }

            // Check if the WeatherDay object contains the specified name.
            if (!weatherDay.Contains(name))
            {
                return false;
            }

            // Check if the index is within the valid range (0 to 23).
            if (index < 0 || index > 23)
            {
                return false;
            }

            // Get the value at the specified index.
            value = weatherDay[name, index];
            return true;
        }

        /// <summary>
        /// Tries to get the value associated with the specified name from the given WeatherYear object at the specified index.
        /// </summary>
        /// <param name="weatherYear">The WeatherYear object containing the weather data.</param>
        /// <param name="name">The name associated with the value.</param>
        /// <param name="index">The index at which to retrieve the value.</param>
        /// <param name="value">The retrieved value, if successful.</param>
        /// <returns>True if the value is retrieved successfully, false otherwise.</returns>
        public static bool TryGetValue(this WeatherYear weatherYear, string name, int index, out double value)
        {
            value = double.NaN;

            if (weatherYear == null || name == null || index == -1)
            {
                return false;
            }

            // Calculate the day index.
            int day = index / 24;

            // Get the WeatherDay object at the calculated day index.
            WeatherDay weatherDay = weatherYear[day];
            if (weatherDay == null)
            {
                return false;
            }

            // Get the value from the WeatherDay object using the remainder of the index divided by 24.
            return TryGetValue(weatherDay, name, index % 24, out value);
        }

        /// <summary>
        /// Tries to get the value associated with the specified WeatherDataType from the given WeatherYear object at the specified index.
        /// </summary>
        /// <param name="weatherYear">The WeatherYear object containing the weather data.</param>
        /// <param name="weatherDataType">The WeatherDataType to search for.</param>
        /// <param name="index">The index at which to retrieve the value.</param>
        /// <param name="value">The retrieved value, if successful.</param>
        /// <returns>True if the value is retrieved successfully, false otherwise.</returns>
        public static bool TryGetValue(this WeatherYear weatherYear, WeatherDataType weatherDataType, int index, out double value)
        {
            // Call the TryGetValue method that takes a string name as a parameter.
            return TryGetValue(weatherYear, weatherDataType.ToString(), index, out value);
        }
    }
}