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
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Name"))
            {
                Name = jObject.Value<string>("Name");
            }

            if(jObject.ContainsKey("Value"))
            {
                JToken jToken = jObject.GetValue("Value");
                if (jToken != null)
                {
                    switch (jToken.Type)
                    {
                        case JTokenType.String:
                            Value = jToken.Value<string>();
                            break;

                        case JTokenType.Float:
                            Value = jToken.Value<double>();
                            break;

                        case JTokenType.Integer:
                            Value = jToken.Value<int>();
                            break;

                        case JTokenType.Boolean:
                            Value = jToken.Value<bool>();
                            break;

                        case JTokenType.Date:
                            Value = jToken.Value<DateTime>();
                            break;

                        case JTokenType.Object:
                            JSAMObjectWrapper jSAMObjectWrapper = new JSAMObjectWrapper((JObject)jToken);
                            IJSAMObject jSAMObject = jSAMObjectWrapper.ToIJSAMObject();
                            if (jSAMObject == null)
                                Value = jSAMObjectWrapper.ToJObject();
                            else
                                Value = jSAMObject;
                            break;

                        case JTokenType.Array:
                            Value = (JArray)jToken;
                            break;
                    }
                }
            }

            if(jObject.ContainsKey("Enum"))
            {
                string text = jObject.Value<string>("Enum");
                if(!string.IsNullOrWhiteSpace(text))
                {
                    Enum enum_Text = null;
                    if(Query.TryGetEnum(text, out TextComparisonType textComparisonType))
                    {
                        enum_Text = textComparisonType;
                    }

                    Enum enum_Number = null;
                    if (Query.TryGetEnum(text, out NumberComparisonType numberComparisonType))
                    {
                        enum_Number = numberComparisonType;
                    }

                    if(enum_Text != null || enum_Number != null)
                    {
                        if(enum_Text != null && enum_Number != null)
                        {
                            if(Query.IsNumeric(Value))
                            {
                                @enum = enum_Number;
                            }
                            else
                            {
                                @enum = enum_Text;
                            }
                        }
                        else if(enum_Text != null)
                        {
                            @enum = enum_Text;
                        }
                        else
                        {
                            @enum = enum_Number;
                        }

                    }
                }
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

            if(Name != null)
            {
                result.Add("Name", Name);
            }

            if(Value != null)
            {
                result.Add("Value", Value as dynamic);
            }

            if(@enum != null)
            {
                result.Add("Enum", @enum.ToString());
            }

            return result;
        }
    }
}
