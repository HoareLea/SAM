using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class SAMInstance : SAMObject
    {
        private SAMType sAMType;

        public SAMInstance(SAMInstance sAMInstance)
            : base(sAMInstance)
        {
            this.sAMType = sAMInstance.sAMType;
        }

        public SAMInstance(SAMInstance sAMInstance, SAMType sAMType)
            : base(sAMInstance)
        {
            this.sAMType = sAMType;
        }

        public SAMInstance(string name, SAMInstance sAMInstance, SAMType sAMType)
            : base(name, sAMInstance)
        {
            this.sAMType = sAMType;
        }

        public SAMInstance(Guid guid, SAMInstance sAMInstance)
            : base(guid, sAMInstance)
        {
            this.sAMType = sAMInstance.sAMType;
        }

        public SAMInstance(Guid guid, SAMType SAMType)
            : base(guid)
        {
            this.sAMType = SAMType;
        }

        public SAMInstance(string name, SAMType SAMType)
            : base(name)
        {
            this.sAMType = SAMType;
        }

        public SAMInstance(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, SAMType SAMType)
            : base(guid, name, parameterSets)
        {
            this.sAMType = SAMType;
        }

        public SAMInstance(JObject jObject)
            : base(jObject)
        {
        }

        public SAMType SAMType
        {
            get
            {
                return sAMType;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("SAMType"))
                sAMType = Create.IJSAMObject<SAMType>(jObject.Value<JObject>("SAMType"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (sAMType != null)
                jObject.Add("SAMType", sAMType.ToJObject());

            return jObject;
        }
    }
}