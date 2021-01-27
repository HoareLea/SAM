using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class ZoneSimulationResult : Result
    {
        public ZoneSimulationResult(string name, string reference)
            : base(name, reference)
        {

        }

        public ZoneSimulationResult(Guid guid, string name, string reference)
            : base(guid, name, reference)
        {

        }

        public ZoneSimulationResult(SpaceSimulationResult spaceSimulationResult)
            : base(spaceSimulationResult)
        {

        }

        public ZoneSimulationResult(JObject jObject)
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