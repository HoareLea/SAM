using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<Category> SubCategories(this Category category)
        {
            if(category == null)
            {
                return null;
            }

            List<Category> result = new List<Category>();

            Category subCategory = category.SubCategory;
            if(subCategory == null)
            {
                return result;
            }

            List<Category> subCategories = SubCategories(subCategory);
            if(subCategories != null)
            {
                result.AddRange(subCategories);
            }

            result.Add(subCategory);

            return result;

        }
    }
}