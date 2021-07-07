using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public class VentilationSystem : MechanicalSystem
    {
        public VentilationSystem(string name, VentilationSystemType ventilationSystemType)
            : base(name, ventilationSystemType)
        {

        }

        public VentilationSystem(VentilationSystem ventilationSystem)
            : base(ventilationSystem)
        {

        }

        public VentilationSystem(JObject jObject)
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