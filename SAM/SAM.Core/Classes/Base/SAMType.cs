using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class SAMType : SAMObject
    {
        public SAMType(Guid guid, string name)
            : base(guid, name)
        {
        }

        public SAMType(Guid guid, string name, IEnumerable<ParameterSet> parameterSets)
            : base(guid, name, parameterSets)
        {
        }

        public SAMType(SAMType sAMType)
            : base(sAMType)
        {
        }

        public SAMType(SAMType sAMType, string name)
            : base(name, Guid.NewGuid(), sAMType)
        {
        }

        public SAMType(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMType(string name)
            : base(name)
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