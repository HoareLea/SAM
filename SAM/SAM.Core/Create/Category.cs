using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static Category Category(this IEnumerable<string> values)
        {
            if(values == null || values.Count() == 0)
            {
                return null;
            }

            Category result = null;
            for (int i = values.Count() - 1; i >= 0; i++)
            {
                string name = values.ElementAt(i);
                if(result == null)
                {
                    result = new Category(name);
                }
                else
                {
                    result = new Category(name, result);
                }
            }

            return result;
        }

        public static Category Category(this string values, char separator)
        {
            if(values == null)
            {
                return null;
            }

            return Category(values.Split(separator));
        }
    }
}