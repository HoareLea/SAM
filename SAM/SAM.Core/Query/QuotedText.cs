namespace SAM.Core
{
    public static partial class Query
    {
        public static string QuotedText(this string @string, out string sufix)
        {
            sufix = null;

            if(string.IsNullOrWhiteSpace(@string))
            {
                return null;
            }

            if(!@string.StartsWith("\""))
            {
                sufix = @string;
                return null;
            }

            int position = 1;
            while (position < @string.Length)
            {
                if (@string[position] == '"')
                {
                    position++;

                    if (position >= @string.Length || @string[position] != '"')
                    {
                        position--;
                        break;
                    }
                }
                position++;
            }

            string result = @string.Substring(1, position - 1);

            sufix = @string.Substring(position + 1);

            return result;
        }
    }
}