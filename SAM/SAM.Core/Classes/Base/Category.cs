using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class Category : IJSAMObject
    {
        private string name;
        private Category subCategory;

        public Category(string name)
        {
            this.name = name;
        }

        public Category(string name, Category subCategory)
        {
            this.name = name;
            this.subCategory = subCategory;
        }

        public Category(Category category)
        {
            if(category != null)
            {
                name = category.name;
                subCategory = category.subCategory == null ? null : new Category(category.subCategory);
            }
        }

        public Category(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Category SubCategory
        {
            get
            {
                return subCategory == null ? null : new Category(subCategory);
            }
        }

        public override string ToString()
        {
            return name;
        }

        public string ToString(string separator)
        {
            if(separator == null)
            {
                separator = string.Empty;
            }

            List<string> values = new List<string>();

            List<Category> categories = this.SubCategories();
            if(categories != null)
            {
                foreach(Category category in categories)
                {
                    string name_Category = category?.Name;
                    if (name_Category == null)
                    {
                        name_Category = string.Empty;

                    }

                    values.Add(name_Category);
                }    
            }

            values.Add(name == null ? string.Empty : name);

            values.Reverse();

            return string.Join(separator, values);
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Name"))
            {
                name = jObject.Value<string>("Name");
            }

            if(jObject.ContainsKey("SubCategory"))
            {
                subCategory = new Category(jObject.Value<JObject>("SubCategory"));
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            
            if(name != null)
            {
                jObject.Add("Name", name);
            }

            if (subCategory != null)
            {
                jObject.Add("SubCategory", subCategory.ToJObject());
            }

            return jObject;
        }
    }
}