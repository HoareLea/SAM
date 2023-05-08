using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class HeatingSystemType : MechanicalSystemType
    {
        //private string description;

        public HeatingSystemType(string name, string description)
            : base(name, description)
        {

        }

        public HeatingSystemType(Guid guid, string name, string description)
            : base(guid, name, description)
        {
        }

        public HeatingSystemType(HeatingSystemType heatingSystemType)
            : base(heatingSystemType)
        {

        }

        public HeatingSystemType(JObject jObject)
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