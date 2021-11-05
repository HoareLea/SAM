using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class OpeningSimulationResult : Result
    {
        public OpeningSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public OpeningSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public OpeningSimulationResult(OpeningSimulationResult openingSimulationResult)
            : base(openingSimulationResult)
        {

        }

        public OpeningSimulationResult(JObject jObject)
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