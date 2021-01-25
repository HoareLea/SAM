using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string ReplaceSpecialCharacters(this string text, string language)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            TextMap textMap = ActiveManager.GetSpecialCharacterMap(language);
            if (textMap == null)
                return text;

            IEnumerable<string> keys = textMap.Keys;
            if (keys == null || keys.Count() == 0)
                return text;

            string result = text;
            foreach(string key in textMap.Keys)
            {
                List<string> values = textMap.GetValues(key);
                if (values == null || values.Count == 0)
                    continue;

                result = result.Replace(key, values[0]);
            }

            return result;
        }
    }
}