using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class TextFilter : Filter
    {
        public TextComparisonType TextComparisonType { get; set; } = TextComparisonType.Equals;
        public string Value { get; set; }

        public TextFilter(JObject jObject)
            :base(jObject)
        {

        }

        public TextFilter(TextFilter textFilter)
            : base(textFilter)
        {
            if(textFilter != null)
            {
                TextComparisonType = textFilter.TextComparisonType;
                Value = textFilter.Value;
            }
        }

        public TextFilter(TextComparisonType textComparisonType, string value)
        {
            TextComparisonType = textComparisonType;
            Value = value;
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("TextComparisonType"))
            {
                TextComparisonType = Query.Enum<TextComparisonType>(jObject.Value<string>("TextComparisonType"));
            }

            if (jObject.ContainsKey("Value"))
            {
                Value = jObject.Value<string>("Value");
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

            result.Add("TextComparisonType", TextComparisonType.ToString());

            if(Value != null)
            {
                result.Add("Value", Value);
            }

            return result;
        }
    }
}
