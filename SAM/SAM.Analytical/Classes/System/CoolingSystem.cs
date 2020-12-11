using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public abstract class CoolingSystem : SAMInstance, ISystem
    {
        public CoolingSystem(string name, CoolingSystemType coolingSystemType)
            : base(name, coolingSystemType)
        {

        }

        public CoolingSystem(CoolingSystem coolingSystem)
            : base(coolingSystem)
        {

        }

        public CoolingSystem(JObject jObject)
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