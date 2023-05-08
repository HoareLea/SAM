using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class VentilationSystemType : MechanicalSystemType
    {
        //private string description;

        public VentilationSystemType(string name, string description)
            : base(name, description)
        {

        }

        public VentilationSystemType(Guid guid, string name, string description)
            : base(guid, name, description)
        {
        }

        public VentilationSystemType(VentilationSystemType ventilationSystemType)
            : base(ventilationSystemType)
        {

        }

        public VentilationSystemType(JObject jObject)
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