using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class SAMObject : ParameterizedSAMObject, ISAMObject
    {
        private Guid guid;
        protected string name;

        public SAMObject(SAMObject sAMObject)
            : base(sAMObject)
        {
            guid = sAMObject.Guid;
            name = sAMObject.Name;
        }

        public SAMObject(string name, SAMObject sAMObject)
            : base(sAMObject)
        {
            guid = sAMObject == null ? Guid.Empty : sAMObject.Guid;
            this.name = name;
        }

        public SAMObject(string name, Guid guid, SAMObject sAMObject)
            : base(sAMObject)
        {
            this.guid = guid;
            this.name = name;
        }

        public SAMObject(Guid guid, SAMObject sAMObject)
            : base(sAMObject)
        {
            this.guid = guid;
            name = sAMObject?.Name;
        }

        public SAMObject(Guid guid, string name, IEnumerable<ParameterSet> parameterSets)
            :base(parameterSets)
        {
            this.guid = guid;
            this.name = name;
        }

        public SAMObject(Guid guid, string name)
            :base()
        {
            this.guid = guid;
            this.name = name;
        }

        public SAMObject()
            : base()
        {
            guid = Guid.NewGuid();
        }

        public SAMObject(JObject jObject)
            :base(jObject)
        {
        }

        public SAMObject(Guid guid)
            :base()
        {
            this.guid = guid;
        }

        public SAMObject(string name)
            :base()
        {
            this.name = name;
            guid = Guid.NewGuid();
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Guid Guid
        {
            get
            {
                return guid;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if(!base.FromJObject(jObject))
            {
                return false;
            }

            name = Query.Name(jObject);
            guid = Query.Guid(jObject);
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if (name != null)
                jObject.Add("Name", name);

            jObject.Add("Guid", guid);

            return jObject;
        }
    }
}