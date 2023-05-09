using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public class JSAMObjectWrapper : ISAMObject
    {
        private JObject jObject;

        public JSAMObjectWrapper(JSAMObjectWrapper jSAMObjectWrapper)
        {
            jObject = jSAMObjectWrapper.jObject.DeepClone() as JObject;
        }
        
        public JSAMObjectWrapper(JObject jObject)
        {
            this.jObject = jObject;
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

        public JSAMObjectWrapper Clone()
        {
            return new JSAMObjectWrapper(this);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            this.jObject = jObject;
            return true;
        }

        public string GetAssemblyName()
        {
            string fullTypeName = Query.FullTypeName(jObject);
            if (string.IsNullOrWhiteSpace(fullTypeName))
            {
                return null;
            }

            if (!Query.TryGetTypeNameAndAssemblyName(fullTypeName, out string typeName, out string assemblyName))
            {
                return null;
            }

            return assemblyName;
        }

        public string GetTypeName()
        {
            string fullTypeName = Query.FullTypeName(jObject);
            if (string.IsNullOrWhiteSpace(fullTypeName))
            {
                return null;
            }

            if (!Query.TryGetTypeNameAndAssemblyName(fullTypeName, out string typeName, out string assemblyName))
            {
                return null;
            }

            return typeName;
        }

        public IJSAMObject ToIJSAMObject()
        {
            return Query.IJSAMObject(jObject);
        }

        public JObject ToJObject()
        {
            return jObject;
        }
    }
}