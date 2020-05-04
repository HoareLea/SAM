using Newtonsoft.Json.Linq;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string Name(this JObject jObject)
        {
            if (jObject == null)
                return null;

            return jObject.Value<string>("Name");
        }

        public static string Name(this Assembly assembly)
        {
            string name = null;

            object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (customAttributes != null && customAttributes.Length > 0)
                name = ((AssemblyTitleAttribute)customAttributes.First()).Title;

            if (name == null)
                name = assembly.ManifestModule.Name;

            return name;
        }
    }
}