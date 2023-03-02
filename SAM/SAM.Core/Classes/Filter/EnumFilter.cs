using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public abstract class EnumFilter<T> : Filter, IEnumFilter where T: Enum
    {
        public T Enum { get; set; }

        public EnumFilter(EnumFilter<T> enumFilter)
            :base(enumFilter)
        {
            if(enumFilter != null)
            {
                Enum = enumFilter.Enum;
            }
        }

        public EnumFilter(JObject jObject)
            :base(jObject)
        {

        }

        public EnumFilter()
            :base()
        {

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

            if(Enum == null && @enum == null)
            {
                return true;
            }

            if(Enum == null || @enum == null)
            {
                return false;
            }

            return Enum.Equals(@enum);
        }

        public abstract bool TryGetEnum(IJSAMObject jSAMObject, out T @enum);

        public override bool FromJObject(JObject jObject)
        {
            if(! base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Enum"))
            {
                string text = jObject.Value<string>("Enum");
                if(!string.IsNullOrWhiteSpace(text))
                {
                    Enum = Query.Enum<T>(text);
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

            if(Enum != null)
            {
                result.Add(Enum.ToString());
            }

            return result;
        }

    }
}
