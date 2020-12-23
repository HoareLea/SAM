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
    }
}