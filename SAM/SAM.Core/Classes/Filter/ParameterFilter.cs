using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public class ParameterFilter : Filter
    {
        private Enum @enum;

        public string Name { get; set; }

        public object Value { get; set; }

        public ParameterFilter(JObject jObject)
            :base(jObject)
        {

        }

        public ParameterFilter(string name, string value, TextComparisonType textComparisonType)
        {
            Name = name;
            Value = value;
            @enum = textComparisonType;
        }

        public ParameterFilter(string name, double value, NumberComparisonType numberComparisonType)
        {
            Name = name;
            Value = value;
            @enum = numberComparisonType;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if(!Query.TryGetValue(jSAMObject, Name, out object value))
            {
                return false;
            }

            bool result = false;

            TextComparisonType? textComparisonType = TextComparisonType;
            if(textComparisonType == null || !textComparisonType.HasValue)
            {
                NumberComparisonType? numberComparisonType = NumberComparisonType;
                if (numberComparisonType == null || !numberComparisonType.HasValue)
                {
                    return false;
                }

                if (!Query.TryConvert(value, out double double_1))
                {
                    return false;
                }

                if (!Query.TryConvert(Value, out double double_2))
                {
                    return false;
                }

                result = Query.Compare(double_1, double_2, numberComparisonType.Value);

            }
            else
            {
                if (!Query.TryConvert(Value, out string string_1))
                {
                    return false;
                }

                if (!Query.TryConvert(value, out string string_2))
                {
                    return false;
                }

                result = Query.Compare(string_1, string_2, textComparisonType.Value);
            }

            if (Inverted)
            {
                result = !result;
            }

            return result;
        }

        public TextComparisonType? TextComparisonType
        {
            get
            {
                if(@enum is TextComparisonType)
                {
                    return (TextComparisonType)@enum;
                }

                return null;
            }

            set
            {
                if(value == null || !value.HasValue)
                {
                    return;
                }

                @enum = value.Value;
            }
        }

        public NumberComparisonType? NumberComparisonType
        {
            get
            {
                if (@enum is NumberComparisonType)
                {
                    return (NumberComparisonType)@enum;
                }

                return null;
            }

            set
            {
                if (value == null || !value.HasValue)
                {
                    return;
                }

                @enum = value.Value;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            return base.FromJObject(jObject);
        }

        public override JObject ToJObject()
        {
            return base.ToJObject();
        }
    }
}
