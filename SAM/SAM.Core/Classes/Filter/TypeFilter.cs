using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class TypeFilter : Filter
    {
        public TypeFilter(JObject jObject)
            : base(jObject)
        {
        }

        public TypeFilter()
        {
        }

        public TypeFilter(TypeFilter typeFilter)
            : base(typeFilter)
        {
            Type = typeFilter?.Type;
        }

        public System.Type Type { get; set; }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Type"))
            {
                string fullTypeName = jObject.Value<string>("Type");
                if (!string.IsNullOrWhiteSpace(fullTypeName))
                {
                    Type = Query.Type(fullTypeName);
                }
            }

            return true;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
            {
                return false;
            }

            if (Type == null)
            {
                return true;
            }

            bool result = Type.IsAssignableFrom(jSAMObject.GetType());
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

            if (Type != null)
            {
                result.Add("Type", Query.FullTypeName(Type));
            }

            return result;
        }
    }
}