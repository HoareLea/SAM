using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public class HeatingSystem : MechanicalSystem
    {
        public HeatingSystem(string id, HeatingSystemType heatingSystemType)
            : base(id, heatingSystemType)
        {

        }

        public HeatingSystem(System.Guid guid, string id, HeatingSystem heatingSystem)
            : base(guid, id, heatingSystem)
        {

        }

        public HeatingSystem(HeatingSystem heatingSystem)
            : base(heatingSystem)
        {

        }

        public HeatingSystem(JObject jObject)
            : base(jObject)
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
                return null;

            return jObject;
        }
    }
}