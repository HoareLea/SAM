using System;

using Newtonsoft.Json.Linq;

using SAM.Core;

namespace SAM.Analytical
{
    public class Construction : SAMType
    {
        public Construction(string name) 
            : base(name)
        {
        }

        public Construction(Guid guid, string name) 
            : base(guid, name)
        {
        }

        public Construction(Construction construction)
            : base(construction)
        {
        }

        public Construction(JObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            return base.FromJObject(jObject);
        }

        public override JObject ToJObject()
        {
            return base.ToJObject();
        }
    }
}
