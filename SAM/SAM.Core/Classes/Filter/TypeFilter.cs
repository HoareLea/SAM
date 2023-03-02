using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public class TypeFilter : Filter
    {
        public System.Type Type { get; set; }

        public TypeFilter(JObject jObject)
            :base(jObject)
        {

        }
        
        public TypeFilter()
        {
        }

        public TypeFilter(TypeFilter typeFilter)
            :base(typeFilter)
        {
            Type = typeFilter?.Type;
        }

        public override bool IsValid(IJSAMObject jSAMObject)
        {
            if(jSAMObject == null)
            {
                return false;
            }

            if(Type == null)
            {
                return true;
            }

            return Type.IsAssignableFrom(jSAMObject.GetType());
        }

        public override bool FromJObject(JObject jObject)
        {
            if(! base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Type"))
            {
                string fullTypeName = jObject.Value<string>("Type");
                if(!string.IsNullOrWhiteSpace(fullTypeName))
                {
                    Type = Query.Type(fullTypeName);
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

            if(Type != null)
            {
                result.Add("Type", Query.FullTypeName(Type));
            }

            return result;
        }
    }
}
