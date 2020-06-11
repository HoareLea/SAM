using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Guid Guid(this JObject jObject)
        {
            Guid guid = Guid(jObject, "Guid");
            if (guid == System.Guid.Empty)
                guid = System.Guid.NewGuid();

            return guid;
        }

        public static Guid Guid(this JObject jObject, string name)
        {
            if (jObject == null || string.IsNullOrWhiteSpace(name))
                return System.Guid.Empty;

            if (!jObject.ContainsKey(name))
                return System.Guid.Empty;

            return Guid(jObject.Value<JToken>(name));
        }

        public static Guid Guid(this JToken jToken)
        {
            if (jToken == null)
                return System.Guid.Empty;
            
            switch (jToken.Type)
            {
                case JTokenType.String:
                    string guidString = jToken.Value<string>();
                    if (!string.IsNullOrWhiteSpace(guidString))
                    {
                        Guid guid_Temp;
                        if (System.Guid.TryParse(guidString, out guid_Temp))
                            return guid_Temp;
                    }
                    break;

                case JTokenType.Guid:
                    return jToken.Value<Guid>();
            }

            return System.Guid.Empty;
        }

        public static Guid Guid(this Assembly assembly)
        {
            Guid guid = System.Guid.Empty;

            object[] customAttributes = assembly.GetCustomAttributes(typeof(GuidAttribute), false);
            if (customAttributes != null && customAttributes.Length > 0)
                System.Guid.TryParse(((GuidAttribute)customAttributes.First()).Value, out guid);

            if (guid.Equals(System.Guid.Empty))
                guid = assembly.ManifestModule.ModuleVersionId;

            return guid;
        }
    }
}