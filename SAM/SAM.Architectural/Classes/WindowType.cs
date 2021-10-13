using Newtonsoft.Json.Linq;

namespace SAM.Architectural
{
    public class WindowType : OpeningType
    {
        public WindowType(WindowType windowType)
            : base(windowType)
        {

        }

        public WindowType(JObject jObject)
            : base(jObject)
        {

        }

        public WindowType(string name)
            : base(name)
        {

        }

        public WindowType(System.Guid guid, string name)
            : base(guid, name)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
                return jObject;

            return jObject;
        }

    }
}
