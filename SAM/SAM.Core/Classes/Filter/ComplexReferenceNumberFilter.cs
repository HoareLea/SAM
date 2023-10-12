using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace SAM.Core
{
    public abstract class ComplexReferenceNumberFilter : ComplexReferenceFilter
    {
        public FilterLogicalOperator FilterLogicalOperator { get; set; } = FilterLogicalOperator.Or;

        public NumberComparisonType NumberComparisonType { get; set; } = NumberComparisonType.Equals;

        public double Value { get; set; }

        public ComplexReferenceNumberFilter(JObject jObject)
            : base(jObject)
        {
        }

        public ComplexReferenceNumberFilter()
            : base()
        {
        }

        public ComplexReferenceNumberFilter(ComplexReferenceNumberFilter complexReferenceNumberFilter)
            : base(complexReferenceNumberFilter)
        {
            if(complexReferenceNumberFilter != null)
            {
                FilterLogicalOperator = complexReferenceNumberFilter.FilterLogicalOperator;
                NumberComparisonType = complexReferenceNumberFilter.NumberComparisonType;
                Value = complexReferenceNumberFilter.Value;
            }
        }
        
        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("FilterLogicalOperator"))
            {
                FilterLogicalOperator = Query.Enum<FilterLogicalOperator>(jObject.Value<string>("FilterLogicalOperator"));
            }

            if (jObject.ContainsKey("NumberComparisonType"))
            {
                NumberComparisonType = Query.Enum<NumberComparisonType>(jObject.Value<string>("NumberComparisonType"));
            }

            if (jObject.ContainsKey("Value"))
            {
                Value = jObject.Value<double>("Value");
            }

            return true;
        }

        protected override bool IsValid(IEnumerable<object> values)
        {
            if(values == null || values.Count() == 0)
            {
                return false;
            }

            foreach(object value in values)
            {
                if(!Query.TryConvert(value, out double number))
                {
                    if(FilterLogicalOperator == FilterLogicalOperator.And)
                    {
                        return false;
                    }

                    continue;
                }

                if(!Query.Compare(number, Value, NumberComparisonType))
                {
                    if (FilterLogicalOperator == FilterLogicalOperator.And)
                    {
                        return false;
                    }

                    continue;
                }

                if (FilterLogicalOperator == FilterLogicalOperator.Or)
                {
                    return true;
                }
            }

            return FilterLogicalOperator == FilterLogicalOperator.And;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            result.Add("FilterLogicalOperator", FilterLogicalOperator.ToString());

            result.Add("NumberComparisonType", NumberComparisonType.ToString());

            if (!double.IsNaN(Value))
            {
                result.Add("Value", Value);
            }

            return result;
        }
    }
}