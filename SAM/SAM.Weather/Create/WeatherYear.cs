using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Create
    {
        public static WeatherYear WeatherYear(int year, Dictionary<string, List<double>> values)
        {
            if(year == -1 || values == null)
            {
                return null;
            }

            WeatherYear result = new WeatherYear(year);

            int max = values.Values.ToList().FindAll(x => x != null).ConvertAll(x => x.Count).Max();
            if(max != -1)
            {
                int day = 0;
                int hour = 0;
                for(int i =0; i < max; i++)
                {
                    Dictionary<string, double> dictionary = new Dictionary<string, double>();
                    foreach(KeyValuePair<string, List<double>> keyValuePair in values)
                    {
                        if(keyValuePair.Value == null || keyValuePair.Value.Count <= i)
                        {
                            continue;
                        }

                        dictionary[keyValuePair.Key] = keyValuePair.Value[i];
                    }

                    result.Add(day, hour, dictionary);

                    hour++;
                    if(hour > 23)
                    {
                        day++;
                        hour = 0;
                    }
                }
            }

            return result;
        }

        public static WeatherYear WeatherYear(int year, Dictionary<WeatherDataType, List<double>> values)
        {
            if(year == -1 || values == null)
            {
                return null;
            }

            Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
            foreach(KeyValuePair<WeatherDataType, List<double>> keyValuePair in values)
            {
                dictionary[keyValuePair.Key.ToString()] = keyValuePair.Value;
            }

            return WeatherYear(year, dictionary);
        }
    }
}