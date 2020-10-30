namespace SAM.Core
{
    public static partial class Query
    {
        public static bool IsHex(this string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(@string, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }
}