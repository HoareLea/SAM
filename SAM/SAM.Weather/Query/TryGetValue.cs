namespace SAM.Weather
{
    public static partial class Query
    {
        public static bool TryGetValue(string text, string name, out string value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string name_Temp = name.Trim();

            string result = text.TrimStart();

            if (!result.ToUpper().StartsWith(name_Temp.ToUpper()))
                return false;

            result = result.Substring(name_Temp.Length);
            result = result.TrimStart();
            if (result.StartsWith(","))
                result = result.Substring(1).TrimStart();

            value = result;
            return true;
        }

        public static bool TryGetValue(this WeatherDay weatherDay, WeatherDataType weatherDataType, int index, out double value)
        {
            value = default;
            if(weatherDay == null || weatherDataType == WeatherDataType.Undefined)
            {
                return false;
            }

            return TryGetValue(weatherDay, weatherDataType.ToString(), index, out value);
        }

        public static bool TryGetValue(this WeatherDay weatherDay, string name, int index, out double value)
        {
            value = default;
            if (weatherDay == null || string.IsNullOrEmpty(name) || index == -1)
            {
                return false;
            }

            if(!weatherDay.Contains(name))
            {
                return false;
            }

            if(index < 0 || index> 23)
            {
                return false;
            }

            value = weatherDay[name, index];
            return true;
        }
    }
}