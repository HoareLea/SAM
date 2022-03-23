using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<T> Search<T>(this IEnumerable<T> items, string text, Func<T, string> func, bool caseSensitive = false)
        {
            if(items == null || func == null || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            SearchWrapper searchWrapper = new SearchWrapper(caseSensitive);

            Dictionary<string, T>  dictionary = new Dictionary<string, T>();
            
            foreach (T item in items)
            {
                if(item == null)
                {
                    continue;
                }

                string value = text == null ? item.ToString() : func.Invoke(item);

                if (searchWrapper.Add(value))
                {
                    dictionary[value] = item;
                }
            }

            List<string> values = searchWrapper.Search(text, true);
            if(values == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(string value in values)
            {
                if(!dictionary.TryGetValue(value, out T item))
                {
                    continue;
                }

                result.Add(item);
            }

            return result;
        }
    }
}