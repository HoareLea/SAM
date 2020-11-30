using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class Profile : SAMObject
    {
        private string category;
        private double[] values;

        public Profile(Profile profile)
            : base(profile)
        {
            values = profile.Values;
            category = profile.category;
        }

        public Profile(string name, string category)
            : base(name)
        {
            this.category = category;
        }

        public Profile(string name, ProfileType profileType)
            : base(name)
        {
            category = profileType.Text();
        }

        public Profile(string name, ProfileGroup profileGroup)
            : base(name)
        {
            category = profileGroup.Text();
        }

        public Profile(string name, string category, params double[] values)
            : base(name)
        {
            ProfileType profileType = category.Enum<ProfileType>();
            if (profileType != ProfileType.Undefined)
            {
                this.category = profileType.Text();
            }
            else
            {
                ProfileGroup profileGroup = category.Enum<ProfileGroup>();
                if (profileGroup != ProfileGroup.Undefined)
                    this.category = profileGroup.Text();
                else
                    this.category = category;
            }
            
            this.values = Core.Query.Clone(values);
        }

        public Profile(string name, ProfileType profileType, params double[] values)
            : base(name)
        {
            category = profileType.Text();
            this.values = Core.Query.Clone(values);
        }

        public Profile(string name, ProfileGroup profileGroup, params double[] values)
            : base(name)
        {
            category = profileGroup.Text();
            this.values = Core.Query.Clone(values);
        }

        public Profile(string name, IEnumerable<double> values)
            : base(name)
        {
            if (values == null)
                return;

            int count = values.Count();

            this.values = new double[count];
            for (int i = 0; i < count; i++)
                this.values[i] = values.ElementAt(i);
        }

        public Profile(JObject jObject)
            : base(jObject)
        {
        }

        public double[] Values
        {
            get
            {
                return Core.Query.Clone(values);
            }
        }

        public int Count
        {
            get
            {
                if (values == null)
                    return -1;

                return values.Length;
            }
        }

        public string Category
        {
            get
            {
                return category;
            }
        }

        public ProfileType ProfileType
        {
            get
            {
                return Core.Query.Enum<ProfileType>(category);
            }
        }

        public ProfileGroup ProfileGroup
        {
            get
            {
                return Core.Query.Enum<ProfileGroup>(category);
            }
        }

        public double Max
        {
            get
            {
                if (values == null)
                    return double.NaN;

                return values.Max();
            }
        }

        public double Min
        {
            get
            {
                if (values == null)
                    return double.NaN;

                return values.Min();
            }
        }

        public double this[int index]
        {
            get
            {
                if (values == null)
                    return double.NaN;

                int count = values.Length;

                if (index < count)
                    return values[index];

                if (count == 1)
                    return values[0];

                return values[(index % count) - 1];

            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Category"))
                category = jObject.Value<string>("Category");

            values = jObject.Value<JArray>("Values")?.ToList<double>()?.ToArray();
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (category != null)
                jObject.Add("Category", category);

            JArray jArray = new JArray();
            foreach (double value in values)
                jArray.Add(value);

            jObject.Add("Values", jArray);

            return jObject;
        }
    }
}