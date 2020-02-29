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

            string guidString = jObject.Value<string>("Guid");
            if (!string.IsNullOrWhiteSpace(guidString))
            {
                Guid guid_Temp;
                if (System.Guid.TryParse(guidString, out guid_Temp))
                    guid = guid_Temp;
            }

            return guid;
        }

        public static Guid Guid(Assembly assembly)
        {
            Guid guid = System.Guid.Empty;

            object[] customAttributes = assembly.GetCustomAttributes(typeof(GuidAttribute), false);
            if (customAttributes != null || customAttributes.Length > 0)
                System.Guid.TryParse(((GuidAttribute)customAttributes.First()).Value, out guid);

            return guid;


        }
    }
}
