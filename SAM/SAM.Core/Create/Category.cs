using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static Category Category(this IEnumerable<string> values, bool trim = true)
        {
            if(values == null || values.Count() == 0)
            {
                return null;
            }

            Category result = null;
            for (int i = values.Count() - 1; i >= 0; i--)
            {
                string name = values.ElementAt(i);
                if(trim)
                {
                    name = name?.Trim();
                }

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

        public static Category Category(this string values, string separator, bool trim = true)
        {
            if(values == null)
            {
                return null;
            }

            return Category(values.Split(new string[] { separator }, System.StringSplitOptions.None), trim);
        }

        public static Category Category(string name, Category parent)
        {
            Category result = new Category(name);
            
            if (parent == null)
            {
                return result;
            }

            List<Category> categories = parent.SubCategories();
            if(categories != null || categories.Count != 0)
            {
                foreach (Category category in categories)
                {
                    result = new Category(category.Name, result);
                }
            }

            result = new Category(parent.Name, result);

            return result;
        }
    }
}