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
            if (jObject == null)
                return System.Guid.Empty;

            Guid guid = System.Guid.NewGuid();

            if (!jObject.ContainsKey("Guid"))
                return guid;

            JToken jToken = jObject.Value<JToken>("Guid");
            switch (jToken.Type)
            {
                case JTokenType.String:
                    string guidString = jToken.Value<string>();
                    if (!string.IsNullOrWhiteSpace(guidString))
                    {
                        Guid guid_Temp;
                        if (System.Guid.TryParse(guidString, out guid_Temp))
                            guid = guid_Temp;
                    }
                    break;

                case JTokenType.Guid:
                    guid = jToken.Value<Guid>();
                    break;
            }
            return guid;
        }

        public static Guid Guid(Assembly assembly)
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