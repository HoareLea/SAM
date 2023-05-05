using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    /// <summary>
    /// Provides functionality to query weather data.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Tries to get location data from the specified lines and index.
        /// </summary>
        /// <param name="lines">The lines containing location data.</param>
        /// <param name="index">The index at which to search for location data.</param>
        /// <param name="city">The city extracted from the location data.</param>
        /// <param name="state">The state extracted from the location data.</param>
        /// <param name="country">The country extracted from the location data.</param>
        /// <param name="dataSource">The data source extracted from the location data.</param>
        /// <param name="wMONumber">The WMO number extracted from the location data.</param>
        /// <param name="latitude">The latitude extracted from the location data.</param>
        /// <param name="longitude">The longitude extracted from the location data.</param>
        /// <param name="timeZone">The time zone extracted from the location data.</param>
        /// <param name="elevation">The elevation extracted from the location data.</param>
        /// <returns>true if location data was successfully parsed, otherwise false.</returns>
        public static bool TryGetLocationData(IEnumerable<string> lines, int index, out string city, out string state, out string country, out string dataSource, out string wMONumber, out double latitude, out double longitude, out double timeZone, out double elevation)
        {
            city = null;
            state = null;
            country = null;
            dataSource = null;
            wMONumber = null;
            latitude = double.NaN;
            longitude = double.NaN;
            timeZone = double.NaN;
            elevation = double.NaN;

            if (lines == null)
                return false;

            if (index < 0 || index >= lines.Count())
                return false;

            string line = lines.ElementAt(index);
            if (string.IsNullOrWhiteSpace(line) || !TryGetValue(line, "LOCATION", out line))
                return false;

            if (line == null)
                return false;

            string[] values = line.Split(',');
            if (values == null || values.Length < 9)
                return false;

            city = values[0];
            state = values[1];
            country = values[2];
            dataSource = values[3];
            wMONumber = values[4];

            if (!double.TryParse(values[5], out latitude))
                latitude = double.NaN;

            if (!double.TryParse(values[6], out longitude))
                longitude = double.NaN;

            if (!double.TryParse(values[7], out timeZone))
                timeZone = double.NaN;

            if (!double.TryParse(values[8], out elevation))
                elevation = double.NaN;

            return true;
        }
    }
}
