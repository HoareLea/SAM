using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryParseGuids(string text, out List<Guid> guids)
        {
            guids = null;

            if(string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string[] separators = new string[] { "\n", "\t", "," };

            List<string> strings = new () { text };
            foreach (string separator in separators)
            {
                List<string> strings_Split = new ();
                foreach (string @string in strings)
                {
                    string[] strings_Temp_Split = @string.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (strings_Temp_Split == null || strings_Temp_Split.Length == 0)
                    {
                        continue;
                    }
                    strings_Split.AddRange(strings_Temp_Split);
                }

                strings = strings_Split;
            }

            strings.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            strings = strings.ConvertAll(x => x.Trim());

            guids = new List<Guid>();
            foreach (string @string in strings)
            {
                string value = null;

                if(string.IsNullOrWhiteSpace(@string))
                {
                    continue;
                }

                value = @string.Trim();
                value = value.Replace("{", string.Empty);
                value = value.Replace("}", string.Empty);
                value = value.Replace("(", string.Empty);
                value = value.Replace(")", string.Empty);
                value = value.Replace("[", string.Empty);
                value = value.Replace("]", string.Empty);
                value = value.Replace(" ", string.Empty);

                if(System.Guid.TryParse(value, out Guid guid))
                {
                    guids.Add(guid);
                }
            }

            return guids.Count != 0;
        }
    }
}