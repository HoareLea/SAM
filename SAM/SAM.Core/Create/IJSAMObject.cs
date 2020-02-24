using System;
using System.Reflection;

using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static IJSAMObject IJSAMObject(this JObject jObject)
        {
            string typeName = Query.TypeName(jObject);
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            Type type = Type.GetType(typeName);
            if (type == null)
                return null;

            ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(JObject) });
            if (constructorInfo == null)
                return null;

            return constructorInfo.Invoke(new object[] { jObject }) as IJSAMObject;
        }

        public static T IJSAMObject<T>(this JObject jObject) where T : IJSAMObject
        {
            string typeName = Query.TypeName(jObject);
            if (string.IsNullOrWhiteSpace(typeName))
                return default;

            Type type = Type.GetType(typeName);
            if (type == null)
                return default;

            ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { typeof(JObject) });
            if (constructorInfo == null)
                return default;

            return (T)constructorInfo.Invoke(new object[] { jObject });
        }

        public static T IJSAMObject<T>(this string json) where T : IJSAMObject
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            JToken jToken = JToken.Parse(json);

            JObject jObject = jToken as JObject;
            if (jObject == null)
                return default;

            return IJSAMObject<T>(jObject);
        }

        public static IJSAMObject IJSAMObject(this string json)
        {
            return IJSAMObject<IJSAMObject>(json);
        }
    }
}
