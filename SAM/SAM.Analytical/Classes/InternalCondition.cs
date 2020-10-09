using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class InternalCondition : SAMObject
    {
        public InternalCondition(InternalCondition internalCondition)
            : base(internalCondition)
        {

        }

        public InternalCondition(Guid guid, InternalCondition internalCondition)
        : base(guid, internalCondition)
        {

        }

        public InternalCondition(Guid guid, string name)
            : base(guid, name)
        {

        }

        public InternalCondition(string name)
            : base(name)
        {
        }

        public InternalCondition(JObject jObject)
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
                return jObject;

            return jObject;
        }
    }
}