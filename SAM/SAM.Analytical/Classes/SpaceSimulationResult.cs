using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class SpaceSimulationResult : SAMObject, IResult
    {
        private string reference;
        
        public SpaceSimulationResult(string name, string reference)
            : base(name)
        {
            this.reference = reference;
        }

        public SpaceSimulationResult(Guid guid, string name, string reference)
            : base(guid, name)
        {
            this.reference = reference;
        }

        public SpaceSimulationResult(SpaceSimulationResult spaceSimulationResult)
            : base(spaceSimulationResult)
        {
            reference = spaceSimulationResult?.reference;
        }

        public SpaceSimulationResult(JObject jObject)
            : base(jObject)
        {
        }

        public string Reference
        {
            get
            {
                return reference;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Reference"))
                reference = jObject.Value<string>("Reference");

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if (reference != null)
                jObject.Add("Reference", reference);

            return jObject;
        }
    }
}