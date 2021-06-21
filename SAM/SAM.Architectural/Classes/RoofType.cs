using Newtonsoft.Json.Linq;

namespace SAM.Architectural
{
    public class RoofType : HostBuildingElementType
    {
        public RoofType(RoofType roofType)
            : base(roofType)
        {

        }

        public RoofType(JObject jObject)
            : base(jObject)
        {

        }

        public RoofType(string name)
            : base(name)
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
