using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public abstract class SAMInstance<T> : SAMObject where T: SAMType
    {
        private T type;

        public SAMInstance(SAMInstance<T> instance)
            : base(instance)
        {
            this.type = instance.Type;
        }

        public SAMInstance(SAMInstance<T> instance, T type)
            : base(instance)
        {
            this.type = type;
        }

        public SAMInstance(string name, SAMInstance<T> instance, T type)
            : base(name, instance)
        {
            this.type = type;
        }

        public SAMInstance(Guid guid, SAMInstance<T> instance)
            : base(guid, instance)
        {
            this.type = instance.Type;
        }

        public SAMInstance(Guid guid, T type)
            : base(guid)
        {
            this.type = type;
        }

        public SAMInstance(Guid guid, string name, T type)
        : base(guid, name)
        {
            this.type = type;
        }

        public SAMInstance(string name, T type)
            : base(name)
        {
            this.type = type;
        }

        public SAMInstance(Guid guid, string name, IEnumerable<ParameterSet> parameterSets, T type)
            : base(guid, name, parameterSets)
        {
            this.type = type;
        }

        public SAMInstance(JObject jObject)
            : base(jObject)
        {
        }

        public T Type
        {
            get
            {
                return type?.Clone();
            }
        }

        public Guid TypeGuid
        {
            get
            {
                if (type == null)
                    return Guid.Empty;

                return type.Guid;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Type"))
            {
                type = Create.IJSAMObject<T>(jObject.Value<JObject>("Type"));
            }
            else
            {
                //TODO: Remove in the future. This is for backward compability only
                if (jObject.ContainsKey("SAMType"))
                {
                    type = Create.IJSAMObject<T>(jObject.Value<JObject>("SAMType"));
                }
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (type != null)
                jObject.Add("Type", type.ToJObject());

            return jObject;
        }
    }
}