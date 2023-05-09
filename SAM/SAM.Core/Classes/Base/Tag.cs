using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace SAM.Core
{
    public class Tag : IJSAMObject
    {
        private object value;

        public Tag(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Tag(Tag tag)
        {
            value = tag?.value;
        }

        public Tag(double value)
        {
            this.value = value;
        }

        public Tag(Guid value)
        {
            this.value = value;
        }

        public Tag(bool value)
        {
            this.value = value;
        }

        public Tag(DateTime value)
        {
            this.value = value;
        }

        public Tag(IJSAMObject value)
        {
            this.value = value;
        }

        public Tag(SAMObject value)
        {
            this.value = value;
        }

        public Tag(string value)
        {
            this.value = value;
        }

        public Tag(Color value)
        {
            this.value = value;
        }

        public object Value
        {
            get
            {
                return value;
            }
        }

        public ValueType ValueType
        {
            get
            {
                return Query.ValueType(Value);
            }
        }

        public static implicit operator Tag(double value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(double? value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(Guid value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(Guid? value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(bool value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(bool? value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(DateTime value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(DateTime? value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(SAMObject value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(string value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(Color value)
        {
            return new Tag(value);
        }

        public static implicit operator Tag(Color? value)
        {
            return new Tag(value);
        }

        public static bool operator !=(Tag tag_1, Tag tag_2)
        {
            return !(tag_1 == tag_2);
        }

        public static bool operator ==(Tag tag_1, Tag tag_2)
        {
            if ((object)tag_1 == null)
                return (object)tag_2 == null;

            return tag_1.Equals(tag_2);
        }

        public override bool Equals(object @object)
        {
            if (@object == null)
            {
                return false;
            }

            if (@object is Tag && this == (Tag)@object)
            {
                return true;
            }

            if (value == @object)
            {
                return true;
            }

            return false;
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (!jObject.ContainsKey("Value"))
            {
                return false;
            }

            if (!jObject.ContainsKey("ValueType"))
            {
                value = jObject.GetValue("Value");
                return true;
            }

            ValueType valueType = Query.Enum<ValueType>(jObject.Value<string>("ValueType"));
            if (valueType == ValueType.Undefined)
            {
                value = null;
                return true;
            }

            switch (valueType)
            {
                case ValueType.Boolean:
                    value = jObject.Value<bool>("Value");
                    return true;

                case ValueType.Color:
                    value = new SAMColor(jObject.Value<JObject>("Value")).ToColor();
                    return true;

                case ValueType.DateTime:
                    value = jObject.Value<DateTime>("Value");
                    return true;

                case ValueType.Double:
                    value = jObject.Value<double>("Value");
                    return true;

                case ValueType.Guid:
                    if (!Enum.TryParse(jObject.Value<string>("Value"), out Guid guid))
                    {
                        return false;
                    }
                    value = guid;
                    return true;

                case ValueType.IJSAMObject:
                    JObject jObject_Temp = jObject.Value<JObject>("Value");
                    if (jObject_Temp == null)
                    {
                        return false;
                    }

                    value = new JSAMObjectWrapper(jObject_Temp).ToIJSAMObject();
                    return true;

                case ValueType.Integer:
                    value = jObject.Value<int>("Value");
                    return true;

                case ValueType.String:
                    value = jObject.Value<string>("Value");
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (value == null)
                {
                    return -1;
                }

                return value.GetHashCode();
            }
        }

        public T GetValue<T>()
        {
            if(value is T)
            {
                return (T)value;
            }

            if(!Query.TryConvert(value, out T result))
            {
                return default(T);
            }

            return result;
        }

        public void SetValue(double value)
        {
            this.value = value;
        }

        public void SetValue(Guid value)
        {
            this.value = value;
        }

        public void SetValue(bool value)
        {
            this.value = value;
        }

        public void SetValue(DateTime value)
        {
            this.value = value;
        }

        public void SetValue(IJSAMObject value)
        {
            this.value = value;
        }

        public void SetValue(string value)
        {
            this.value = value;
        }

        public void SetValue(Color value)
        {
            this.value = value;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            ValueType valueType = ValueType;
            jObject.Add("ValueType", valueType.ToString());
            
            if(valueType != ValueType.Undefined)
            {
                object value = null;
                switch(valueType)
                {
                    case ValueType.Boolean:
                        if(Query.TryConvert(Value, out bool @bool))
                        {
                            value = @bool;
                        }
                        break;
                    
                    case ValueType.Color:
                        if (Query.TryConvert(Value, out Color color))
                        {
                            value = new SAMColor(color).ToJObject();
                        }
                        break;
                    
                    case ValueType.DateTime:
                        if (Query.TryConvert(Value, out DateTime dateTime))
                        {
                            value = dateTime;
                        }
                        break;

                    case ValueType.Double:
                        if (Query.TryConvert(Value, out double @double))
                        {
                            value = @double;
                        }
                        break;

                    case ValueType.Guid:
                        if (Query.TryConvert(Value, out Guid @guid))
                        {
                            value = @guid;
                        }
                        break;

                    case ValueType.IJSAMObject:
                        value = ((IJSAMObject)Value).ToJObject();
                        break;

                    case ValueType.Integer:
                        if (Query.TryConvert(Value, out int @int))
                        {
                            value = @int;
                        }
                        break;

                    case ValueType.String:
                        if (Query.TryConvert(Value, out string @string))
                        {
                            value = @string;
                        }
                        break;
                }

                if(value != null)
                {
                    jObject.Add("Value", value as dynamic);
                }
            }

            return jObject;
        }
    }
}
