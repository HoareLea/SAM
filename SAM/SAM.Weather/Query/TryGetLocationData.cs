using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static bool TryGetLocationData(IEnumerable<string> lines, int index, out string city, out string state, out string country, out string dataSource, out string wMONumber, out double latitude, out double longitude, out int timeZone, out double elevation)
        {
            city = null;
            state = null;
            country = null;
            dataSource = null;
            wMONumber = null;
            latitude = double.NaN;
            longitude = double.NaN;
            timeZone = int.MinValue;
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
            if (values == null || values.Length < 10)
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

            if (!int.TryParse(values[7], out timeZone))
                timeZone = int.MinValue;

            if (!double.TryParse(values[8], out elevation))
                elevation = double.NaN;

            return true;
        }

    }
}