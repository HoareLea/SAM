using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class SpaceSimulationResult : Result
    {
        public SpaceSimulationResult(string name, string reference)
            : base(name, reference)
        {

        }

        public SpaceSimulationResult(Guid guid, string name, string reference)
            : base(guid, name, reference)
        {

        }

        public SpaceSimulationResult(SpaceSimulationResult spaceSimulationResult)
            : base(spaceSimulationResult)
        {

        }

        public SpaceSimulationResult(Guid guid, SpaceSimulationResult spaceSimulationResult)
            : base(guid, spaceSimulationResult)
        {

        }

        public SpaceSimulationResult(JObject jObject)
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