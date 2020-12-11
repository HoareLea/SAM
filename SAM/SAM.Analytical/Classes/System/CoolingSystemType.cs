using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class CoolingSystemType : MechanicalSystemType
    {
        private string description;

        public CoolingSystemType(string name, string description)
            : base(name, description)
        {

        }

        public CoolingSystemType(Guid guid, string name, string description)
            : base(guid, name, description)
        {
        }

        public CoolingSystemType(CoolingSystemType coolingSystemType)
            : base(coolingSystemType)
        {

        }

        public CoolingSystemType(JObject jObject)
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