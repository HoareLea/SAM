using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public abstract class MechanicalSystem : SAMInstance<MechanicalSystemType>, ISystem
    {
        public MechanicalSystem(string name, MechanicalSystemType mechanicalSystemType)
            : base(name, mechanicalSystemType)
        {

        }

        public MechanicalSystem(MechanicalSystem mechanicalSystem)
            : base(mechanicalSystem)
        {

        }

        public MechanicalSystem(JObject jObject)
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