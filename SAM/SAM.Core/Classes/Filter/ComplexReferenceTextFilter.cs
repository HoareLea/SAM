using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public abstract class ComplexReferenceTextFilter : ComplexReferenceFilter
    {
        public FilterLogicalOperator FilterLogicalOperator { get; set; } = FilterLogicalOperator.Or;

        public TextComparisonType TextComparisonType { get; set; } = TextComparisonType.Equals;

        public string Value { get; set; } = null;

        public bool CaseSensitive { get; set; } = true;

        public ComplexReferenceTextFilter(JObject jObject)
            : base(jObject)
        {
        }

        public ComplexReferenceTextFilter()
            : base()
        {
        }

        public ComplexReferenceTextFilter(ComplexReferenceTextFilter complexReferenceTextFilter)
            : base(complexReferenceTextFilter)
        {
            if(complexReferenceTextFilter != null)
            {
                FilterLogicalOperator = complexReferenceTextFilter.FilterLogicalOperator;
                TextComparisonType = complexReferenceTextFilter.TextComparisonType;
                Value = complexReferenceTextFilter.Value;
                CaseSensitive = complexReferenceTextFilter.CaseSensitive;
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

            if (jObject.ContainsKey("TextComparisonType"))
            {
                TextComparisonType = Query.Enum<TextComparisonType>(jObject.Value<string>("TextComparisonType"));
            }

            if (jObject.ContainsKey("Value"))
            {
                Value = jObject.Value<string>("Value");
            }

            if (jObject.ContainsKey("CaseSensitive"))
            {
                CaseSensitive = jObject.Value<bool>("CaseSensitive");
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
                if(!Query.TryConvert(value, out string text))
                {
                    if(FilterLogicalOperator == FilterLogicalOperator.And)
                    {
                        return false;
                    }

                    continue;
                }

                if(!Query.Compare(text, Value, TextComparisonType, CaseSensitive))
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

            result.Add("TextComparisonType", TextComparisonType.ToString());

            result.Add("CaseSensitive", CaseSensitive);

            if (Value != null)
            {
                result.Add("Value", Value);
            }

            return result;
        }
    }
}