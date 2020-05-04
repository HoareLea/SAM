using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

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

        public bool Add(string name, IJSAMObject value)
        {
            return Add(name, (object)value);
        }

        public bool Add(string name, string value)
        {
            return Add(name, (object)value);
        }

        public bool Contains(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            ParameterSet parameterSet = GetParameterSet(Assembly.GetAssembly(this.GetType()));
            if (parameterSet == null)
                return false;

            return parameterSet.Contains(name);
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            Assembly assembly = Assembly.GetAssembly(this.GetType());

            ParameterSet parameterSet = GetParameterSet(assembly);
            if (parameterSet == null)
                return false;

            if (!parameterSet.Contains(name))
                return false;

            object @object = parameterSet.ToObject(name);

            if (@object == null && value == null)
                return true;

            if (@object is T)
            {
                value = (T)@object;
                return true;
            }

            return false;
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