using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class Profile : SAMObject
    {
        private double[] values;

        public Profile(Profile profile)
            : base(profile)
        {
            values = profile.Values;
        }

        public Profile(string name)
            : base(name)
        {
        }

        public Profile(string name, params double[] values)
            : base(name)
        {
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

            return jObject;
        }
    }
}