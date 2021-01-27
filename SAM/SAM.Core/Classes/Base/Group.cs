using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public class Group : SAMObject
    {
        public Group(string name)
            : base(name)
        {

        }

        public Group(Guid guid, string name)
            : base(guid, name)
        {

        }

        public Group(Group group)
            : base(group)
        {

        }

        public Group(JObject jObject)
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