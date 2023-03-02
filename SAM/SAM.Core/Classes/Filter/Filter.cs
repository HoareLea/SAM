using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class Filter : IFilter
    {
        public bool Inverted { get; set; } = false;

        public Filter()
        {
        }

        public Filter(bool inverted)
        {
            Inverted = inverted;
        }

        public Filter(Filter filter)
        {
            if(filter != null)
            {
                Inverted = filter.Inverted;
            }
        }

        public Filter(JObject jObject)
        {
            FromJObject(jObject);
        }

        public abstract bool IsValid(IJSAMObject jSAMObject);

        public virtual bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Inverted"))
            {
                Inverted = jObject.Value<bool>("Inverted");
            }
            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            jObject.Add("Inverted", Inverted);
            return jObject;
        }
    }
}
