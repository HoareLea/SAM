using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class Modifier : IModifier
    {
        public Modifier()
        {

        }

        public Modifier(Modifier modifier)
        {

        }

        public Modifier(JObject jObject)
        {
            FromJObject(jObject);
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            return jObject;
        }
    }
}