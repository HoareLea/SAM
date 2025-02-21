using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class IndexedDoublesModifier : IndexedSimpleModifier
    {
        public IndexedDoubles Values { get; set; }

        public IndexedDoublesModifier(ArithmeticOperator arithmeticOperator, IndexedDoubles values)
        {
            ArithmeticOperator = arithmeticOperator;
            Values = values == null ? null : new IndexedDoubles(values);
        }

        public IndexedDoublesModifier(IndexedDoublesModifier indexedDoublesModifier)
            : base(indexedDoublesModifier)
        {
            if(indexedDoublesModifier != null)
            {
                Values = indexedDoublesModifier?.Values == null ? null : new IndexedDoubles(indexedDoublesModifier.Values);
            }
        }

        public IndexedDoublesModifier(JObject jObject)
            :base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("Values"))
            {
                Values = Core.Query.IJSAMObject<IndexedDoubles>(jObject.Value<JObject>("Values"));
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            if(Values != null)
            {
                result.Add("Values", Values.ToJObject());
            }

            return result;
        }

        public override bool ContainsIndex(int index)
        {
            if(Values == null)
            {
                return false;
            }

            return Values.ContainsIndex(index);
        }

        public override double GetCalculatedValue(int index, double value)
        {
            if (Values == null)
            {
                return value;
            }

            if (!Values.TryGetValue(index, out double value_Temp))
            {
                return value;
            }

            if(double.IsNaN(value_Temp))
            {
                return double.NaN;
            }

            return Core.Query.Calculate(ArithmeticOperator, value, value_Temp);
        }
    }
}