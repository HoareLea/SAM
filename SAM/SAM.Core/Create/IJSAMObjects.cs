using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<T> IJSAMObjects<T>(this JArray jArray) where T : IJSAMObject
        {
            if (jArray == null)
                return null;

            List<T> result = new List<T>();

            foreach (JObject jObject in jArray)
                result.Add(IJSAMObject<T>(jObject));

            return result;
        }

        public static List<T> IJSAMObjects<T>(this string json) where T : IJSAMObject
        {
            if (string.IsNullOrEmpty(json))
                return default;

            JArray jArray = Query.JArray(json);
            if (jArray == null)
                return default;

            return IJSAMObjects<T>(jArray);
        }
    }
}