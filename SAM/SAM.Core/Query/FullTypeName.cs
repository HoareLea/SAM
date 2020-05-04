using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string FullTypeName(this JObject jObject)
        {
            if (jObject == null)
                return null;

            return jObject.Value<string>("_type");
        }

        public static string FullTypeName(Type type)
        {
            if (type == null)
                return null;

            return string.Format("{0},{1}", type.FullName, type.Assembly.GetName().Name);
        }

        public static string FullTypeName(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return null;

            return FullTypeName(jSAMObject.GetType());
        }
    }
}