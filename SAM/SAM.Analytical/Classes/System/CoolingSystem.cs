﻿using Newtonsoft.Json.Linq;
namespace SAM.Analytical
{
    public class CoolingSystem : MechanicalSystem
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