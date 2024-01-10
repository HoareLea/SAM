using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static int IndexOf(this string value, char @char, int startIndex = 0, bool skipQuotes = true)
        {

            return IndexOf(value, @char.ToString(), startIndex, skipQuotes);
        }

        public static int IndexOf(this string value, string text, int startIndex = 0, bool skipQuotes = true)
        {
            if (value == null || value.Length == 0)
            {
                return -1;
            }

            if (!skipQuotes)
            {
                return value.IndexOf(text, startIndex);
            }

            int result = value.IndexOf(text, startIndex);

            while (result != -1 && value.Substring(0, result).Count(x => x == '"') % 2 != 0)
            {
                result = value.IndexOf(text, result + 1);
            }

            return result;
        }
    }
}