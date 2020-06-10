using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class SAMModel : SAMObject
    {
        public SAMModel(Guid guid, string name)
            : base(guid, name)
        {
        }

        public SAMModel(Guid guid, string name, IEnumerable<ParameterSet> parameterSets)
            : base(guid, name, parameterSets)
        {
        }

        public SAMModel(SAMModel sAMModel)
            : base(sAMModel)
        {
        }

        public SAMModel(SAMModel sAMModel, string name)
            : base(name, Guid.NewGuid(), sAMModel)
        {
        }

        public SAMModel(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMModel(string name)
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