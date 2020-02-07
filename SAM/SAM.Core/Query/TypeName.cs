using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string TypeName(this JObject jObject)
        {
            if (jObject == null)
                return null;

            return jObject.Value<string>("_type");
        }

        public static string TypeName(Type type)
        {
            if (type == null)
                return null;

            return string.Format("{0},{1}", type.FullName, type.Assembly.GetName().Name);
        }

        public static string TypeName(IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return null;

            return TypeName(jSAMObject.GetType());
        }
    }
}
