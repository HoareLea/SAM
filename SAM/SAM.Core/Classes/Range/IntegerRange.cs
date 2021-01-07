using Newtonsoft.Json.Linq;

using System.Collections.Generic;

namespace SAM.Core
{
    public class IntegerRange : IJSAMObject
    {
        private int min;
        private int max;

        public IntegerRange(int value_1, int value_2)
        {
            min = System.Math.Min(value_1, value_2);
            max = System.Math.Max(value_1, value_2);
        }

        public IntegerRange(int value)
        {
            min = value;
            max = value;
        }

        public IntegerRange(IEnumerable<int> values)
        {
            min = int.MaxValue;
            max = int.MinValue;

            if(values != null)
            {
                foreach(int value in values)
                {
                    if (value < min)
                        min = value;

                    if (value > max)
                        max = value;
                }
            }
        }

        public IntegerRange(JObject jObject)
        {
            FromJObject(jObject);
        }

        public int Max
        {
            get
            {
                return max;
            }
        }

        public int Min
        {
            get
            {
                return min;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            max = jObject.Value<int>("Max");
            min = jObject.Value<int>("Min");

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            jObject.Add("Max", max);
            jObject.Add("Min", min);

            return jObject;
        }

        public bool In(int value)
        {
            return value < max && value > min;
        }

        public bool Out(int value)
        {
            return !In(value);
        }
    }
}