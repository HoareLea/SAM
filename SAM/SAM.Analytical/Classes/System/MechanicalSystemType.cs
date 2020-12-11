using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public abstract class MechanicalSystemType : SAMType, ISystemType
    {
        private string description;

        public MechanicalSystemType(string name, string description)
            : base(name)
        {
            this.description = description;
        }

        public MechanicalSystemType(Guid guid, string name, string description)
            : base(guid, name)
        {
            this.description = description;
        }

        public MechanicalSystemType(MechanicalSystemType mechanicalSystemType)
            : base(mechanicalSystemType)
        {
            description = mechanicalSystemType.description;
        }

        public MechanicalSystemType(JObject jObject)
            : base(jObject)
        {
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("Description"))
                description = jObject.Value<string>("Description");

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if(description != null)
                jObject.Add("Description", description);

            return jObject;
        }
    }
}