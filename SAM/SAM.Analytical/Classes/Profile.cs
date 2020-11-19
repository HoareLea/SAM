using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class Profile : SAMObject
    {
        private string group = ProfileType.Other.Text();
        private double[] values;

        public Profile(Profile profile)
            : base(profile)
        {
            values = profile.Values;
            group = profile.group;
        }

        public Profile(string name, string group)
            : base(name)
        {
            this.group = group;
        }

        public Profile(string name, ProfileType profileType)
            : base(name)
        {
            group = profileType.Text();
        }

        public Profile(string name, ProfileType profileType, params double[] values)
            : base(name)
        {
            group = profileType.Text();

            if (values == null)
                return;

            this.values = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
                this.values[i] = values[i];
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
                if (values == null)
                    return null;

                double[] result = new double[values.Length];
                for (int i = 0; i < result.Length; i++)
                    result[i] = values[i];
                
                return result;
            }
        }

        public string Group
        {
            get
            {
                return group;
            }
        }

        public ProfileType ProfileType
        {
            get
            {
                return Query.ProfileType(group);
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

            values = jObject.Value<JArray>("Values")?.ToList<double>()?.ToArray();

            if (jObject.ContainsKey("Group"))
                group = jObject.Value<string>("Group");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            JArray jArray = new JArray();
            foreach (double value in values)
                jArray.Add(value);

            jObject.Add("Values", jArray);

            if (group != null)
                jObject.Add("Group", group);
            return jObject;
        }
    }
}