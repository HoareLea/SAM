using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace SAM.Core
{
    public class JSAMObjectWrapper : ISAMObject
    {
        private JObject jObject;

        public JSAMObjectWrapper(JObject jObject)
        {
            this.jObject = jObject;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            this.jObject = jObject;
            return true;
        }

        public JObject ToJObject()
        {
            return jObject;
        }

        public IJSAMObject ToIJSAMObject()
        {
            return Query.IJSAMObject(jObject);
        }

        public Guid Guid
        {
            get
            {
                if (jObject == null)
                    return Guid.Empty;

                return Query.Guid(jObject);
            }
        }

        public string Name
        {
            get
            {
                if (jObject == null)
                    return null;

                return Query.Name(jObject);
            }
        }

        public string GetTypeName()
        {
            string fullTypeName = Query.FullTypeName(jObject);
            if (string.IsNullOrWhiteSpace(fullTypeName))
                return null;

            string typeName = null;
            string assemblyName = null;
            if (!Query.TryGetTypeNameAndAssemblyName(fullTypeName, out typeName, out assemblyName))
                return null;

            return typeName;
        }

        public string GetAssemblyName()
        {
            string fullTypeName = Query.FullTypeName(jObject);
            if (string.IsNullOrWhiteSpace(fullTypeName))
                return null;

            string typeName = null;
            string assemblyName = null;
            if (!Query.TryGetTypeNameAndAssemblyName(fullTypeName, out typeName, out assemblyName))
                return null;

            return assemblyName;
        }
    }
}