using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;


namespace SAM.Core
{
    public class Setting : SAMObject
    {
        private DateTime created;
        private DateTime updated;
       
        public Setting()
            : base()
        {
            created = DateTime.Now;
            updated = DateTime.Now;
        }

        public Setting(Assembly assembly)
            : base(Query.Guid(assembly), Query.Name(assembly))
        {
            created = DateTime.Now;
            updated = DateTime.Now;
        }

        public Setting(JObject jObject)
        {
            FromJObject(jObject);
        }

        public DateTime Created
        {
            get
            {
                return created;
            }
        }

        public DateTime Updated
        {
            get
            {
                return updated;
            }
        }

        public void Clear()
        {

        }

        public bool Add(string name, IJSAMObject jSAMObject)
        {
            return Add(name, (object)jSAMObject);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!FromJObject(jObject))
                return false;

            created = jObject.Value<DateTime>("Created");
            updated = jObject.Value<DateTime>("Updated");
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Created", created);
            jObject.Add("Updated", DateTime.Now);
            return jObject;
        }


        private bool Add(string name, object @object)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            Assembly assembly = Assembly.GetAssembly(this.GetType());

            ParameterSet parameterSet = GetParameterSet(assembly);

            if (parameterSet == null)
            {
                parameterSet = new ParameterSet(assembly);
                Add(parameterSet);
            }

            return parameterSet.Add(name, @object as dynamic);
        }
    }
}
