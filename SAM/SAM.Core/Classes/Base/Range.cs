using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class Range<T> : IJSAMObject
    {
        private T min;
        private T max;

        public Range(T value_1, T value_2)
        {
            min = System.Math.Min(value_1 as dynamic, value_2 as dynamic);
            max = System.Math.Max(value_1 as dynamic, value_2 as dynamic);
        }

        public Range(T value)
        {
            min = value;
            max = value;
        }

        public Range(IEnumerable<T> values)
        {
            min = default;
            max = default;

            if(values != null)
            {
                List<T> list = values.ToList();
                max = list.Max();
                min = list.Min();
            }
        }

        public Range(Range<T> range)
        {
            min = range.min;
            max = range.max;
        }

        public Range(JObject jObject)
        {
            FromJObject(jObject);
        }

        public T Max
        {
            get
            {
                return max;
            }
        }

        public T Min
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

            max = jObject.Value<T>("Max");
            min = jObject.Value<T>("Min");

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            jObject.Add("Max", max as dynamic);
            jObject.Add("Min", min as dynamic);

            return jObject;
        }

        public bool In(T value)
        {
            return (value as dynamic) <= (max as dynamic) && (value as dynamic) >= (min as dynamic);
        }

        public bool Out(T value)
        {
            return !In(value);
        }

        public override bool Equals(object @object)
        {
            if (ReferenceEquals(this, null))
                return ReferenceEquals(@object, null) ? true : false;

            return @object is Range<T> range && range.max.Equals(max) && range.min.Equals(min);
        }

        public override int GetHashCode()
        {
            return (min, max).GetHashCode();
        }

        public static bool operator ==(Range<T> range, object @object)
        {
            if (ReferenceEquals(range, null))
                return ReferenceEquals(@object, null) ? true : false;

            return range.Equals(@object);
        }

        public static bool operator !=(Range<T> range, object @object)
        {
            return !(range == @object);
        }
    }
}