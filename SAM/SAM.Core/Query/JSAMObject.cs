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
            {
                return null;
            }

            string fullTypeName = FullTypeName(jObject);
            if (string.IsNullOrWhiteSpace(fullTypeName))
            {
                return new JSAMObjectWrapper(jObject);
            }

            Type type = Type(fullTypeName);
            if (type == null)
            {
                return new JSAMObjectWrapper(jObject);
            }

            ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(JObject) });
            if (constructorInfo == null)
            {
                constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(JObject) }, null);
            }

            if(constructorInfo == null)
            {
                return new JSAMObjectWrapper(jObject);
            }

            return constructorInfo.Invoke(new object[] { jObject }) as IJSAMObject;
        }

        public static T IJSAMObject<T>(this JObject jObject) where T : IJSAMObject
        {
            IJSAMObject jSAMObject = IJSAMObject(jObject);
            if(jSAMObject == null)
            {
                return default;
            }

            if(!(jSAMObject is T))
            {
                return default;
            }

            return (T)jSAMObject;
        }
    }
}