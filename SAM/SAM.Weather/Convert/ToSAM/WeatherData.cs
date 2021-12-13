using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Convert
    {
        public static WeatherData ToSAM(string pathEPW)
        {
            if (string.IsNullOrWhiteSpace(pathEPW) || !System.IO.File.Exists(pathEPW))
                return null;

            List<string> lines = System.IO.File.ReadAllLines(pathEPW)?.ToList();
            if (lines == null || lines.Count < 10)
                return null;

            return ToSAM(lines);
        }

        public static WeatherData ToSAM(IEnumerable<string> lines)
        {
            string city = null;
            string state = null;
            string country = null;
            string dataSource = null;
            string wMONumber = null;
            double latitude = double.NaN;
            double longitude = double.NaN;
            int timeZone = int.MinValue;
            double elevation = double.NaN;

            bool @break = true;

            for (int i = 0; i <= 8; i++)
            {
                if (Query.TryGetLocationData(lines, i, out city, out state, out country, out dataSource, out wMONumber, out latitude, out longitude, out timeZone, out elevation))
                {
                    @break = false;
                    break;
                }
            }

            if (@break)
                return null;

            Core.SAMCollection<GroundTemperature> groundTemperatures = null;
            for (int i = 0; i <= 8; i++)
            {
                List<GroundTemperature> groundTemperatures_Temp = null;
                if (Query.TryGetGroundTemperatures(lines, i, out groundTemperatures_Temp))
                {
                    groundTemperatures = new Core.SAMCollection<GroundTemperature>(groundTemperatures_Temp);
                    break;
                }
                    
            }

            string comments_1 = null;
            for (int i = 0; i <= 8; i++)
            {
                if (Query.TryGetValue(lines.ElementAt(i), "COMMENTS 1", out comments_1))
                    break;
            }

            string comments_2 = null;
            for (int i = 0; i <= 8; i++)
            {
                if (Query.TryGetValue(lines.ElementAt(i), "COMMENTS 2", out comments_2))
                    break;
            }

            lines = lines.ToList().GetRange(8, lines.Count() - 8);
            if (lines.Count() < 1)
                return null;

            WeatherData result = new WeatherData(string.Format("{2}_{1}_{0}, {3}, {4}", city, state, country, wMONumber, dataSource), comments_1, latitude, longitude, elevation);
            result.SetValue(WeatherDataParameter.City, city);
            result.SetValue(WeatherDataParameter.Comments_1, comments_1);
            result.SetValue(WeatherDataParameter.Comments_2, comments_2);
            result.SetValue(WeatherDataParameter.Country, country);
            result.SetValue(WeatherDataParameter.DataSource, dataSource);
            result.SetValue(WeatherDataParameter.GroundTemperatures, groundTemperatures);
            result.SetValue(WeatherDataParameter.State, state);
            result.SetValue(WeatherDataParameter.TimeZone, timeZone);
            result.SetValue(WeatherDataParameter.WMONumber, wMONumber);

            foreach (string line in lines)
            {
                DateTime dateTime;
                Dictionary<string, double> dictionary;
                if (!Query.TryGetData(line, out dateTime, out dictionary, false, 2018))
                    continue;

                result.Add(dateTime, dictionary);
            }

            return result;
        }
    }
}