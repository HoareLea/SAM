﻿using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public abstract class EnumFilter<T> : Filter, IEnumFilter where T: Enum
    {
        public EnumFilter(EnumFilter<T> enumFilter)
            : base(enumFilter)
        {
            if (enumFilter != null)
            {
                Value = enumFilter.Value;
            }
        }

        public EnumFilter(JObject jObject)
            : base(jObject)
        {

        }

        public EnumFilter()
            : base()
        {

        }

        public Enum Enum
        {
            get
            {
                return Value;
            }

            set
            {
                if (value is T)
                {
                    Value = (T)value;
                }
            }
        }

        public T Value { get; set; }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Enum"))
            {
                string text = jObject.Value<string>("Enum");
                if (!string.IsNullOrWhiteSpace(text))
                {
                    Value = Query.Enum<T>(text);
                }
            }

            return true;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if(jSAMObject == null)
            {
                return false;
            }

            if(!TryGetEnum(jSAMObject, out T @enum))
            {
                return false;
            }

            if(@enum == null)
            {
                return false;
            }

            if(Value == null && @enum == null)
            {
                return true;
            }

            if(Value == null || @enum == null)
            {
                return false;
            }

            bool result = Value.Equals(@enum);

            if (Inverted)
            {
                result = !result;
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return result;
            }

            if (Value != null)
            {
                result.Add(Value.ToString());
            }

            return result;
        }

        public abstract bool TryGetEnum(IJSAMObject jSAMObject, out T @enum);
    }
}
