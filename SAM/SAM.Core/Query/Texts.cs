using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<string> Texts(string text, char openSymbol = '[', char closeSymbol = ']')
        {
            if (text == null)
                return null;

            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(text))
                return result;

            int startIndex = 0;
            int index = text.IndexOf(openSymbol, startIndex);
            while(index != -1)
            {
                startIndex = text.IndexOf(closeSymbol, index);

                int count_openSymbol = -1;
                int count_closeSymbol = -1;

                string value = null;

                do
                {
                    if (startIndex == -1)
                    {
                        index = -1;
                        break;
                    }

                    int length = startIndex - index - 1;
                    if (length < 0)
                    {
                        index = -1;
                        break;
                    }


                    if (length > 0)
                        value = text.Substring(index + 1, length);
                    else
                        value = string.Empty;

                    count_openSymbol = value.Count(x => x == openSymbol);
                    count_closeSymbol = value.Count(x => x == closeSymbol);
                    if (count_closeSymbol != count_openSymbol)
                    {
                        if (startIndex == text.Length - 1)
                        {
                            result.Add(value);
                            return result;
                        }

                        startIndex = text.IndexOf(closeSymbol, startIndex + 1);
                    }
                }
                while (count_openSymbol != count_closeSymbol);

                if (value != null)
                    result.Add(value);

                if (startIndex != -1)
                    index = text.IndexOf(openSymbol, startIndex);
                else
                    index = -1;
            }

            return result;
        }

        public static List<string> Texts(string text, bool unique, char openSymbol = '[', char closeSymbol = ']')
        {
            List<string> texts = Texts(text, openSymbol, closeSymbol);
            if (texts == null || texts.Count < 2)
                return texts;

            return texts.Distinct().ToList();
        }
    }
}