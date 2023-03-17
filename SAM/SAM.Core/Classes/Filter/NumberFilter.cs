using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class NumberFilter : Filter
    {
        public NumberComparisonType NumberComparisonType { get; set; } = NumberComparisonType.Equals;
        
        public double Value { get; set; }

        public NumberFilter(JObject jObject)
            :base(jObject)
        {

        }

        public NumberFilter(NumberFilter numberFilter)
            :base(numberFilter)
        {
            if(numberFilter != null)
            {
                NumberComparisonType = numberFilter.NumberComparisonType;
                Value = numberFilter.Value;
            }
        }

        public NumberFilter(NumberComparisonType numberComparisonType, double value)
        {
            NumberComparisonType = numberComparisonType;
            Value = value;
        }

        public abstract bool TryGetNumber(IJSAMObject jSAMObject, out double number);

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (!TryGetNumber(jSAMObject, out double number))
            {
                return false;
            }

            bool result = Query.Compare(number, Value, NumberComparisonType);
            if (Inverted)
            {
                result = !result;
            }

            return result;
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("NumberComparisonType"))
            {
                NumberComparisonType = Query.Enum<NumberComparisonType>(jObject.Value<string>("NumberComparisonType"));
            }

            if (jObject.ContainsKey("Value"))
            {
                Value = jObject.Value<double>("Value");
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return result;
            }

            result.Add("NumberComparisonType", NumberComparisonType.ToString());

            if(!double.IsNaN(Value))
            {
                result.Add("Value", Value);
            }

            return result;
        }
    }
}
