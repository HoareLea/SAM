using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static IJSAMObject IJSAMObject(this JObject jObject)
        {
            if (jObject == null)
                return null;

            string fullTypeName = Query.FullTypeName(jObject);
            if (string.IsNullOrWhiteSpace(fullTypeName))
                return new JSAMObjectWrapper(jObject);

            Type type = Type.GetType(fullTypeName);
            if (type == null)
                return new JSAMObjectWrapper(jObject);

            ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(JObject) });
            if (constructorInfo == null)
                return new JSAMObjectWrapper(jObject);

            return constructorInfo.Invoke(new object[] { jObject }) as IJSAMObject;
        }
    }
}