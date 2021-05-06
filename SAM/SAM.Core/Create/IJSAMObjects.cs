using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<T> IJSAMObjects<T>(this JArray jArray) where T : IJSAMObject
        {
            if (jArray == null)
                return null;

            List<T> result = Enumerable.Repeat<T>(default, jArray.Count).ToList();

            Parallel.For(0, jArray.Count, (int i) =>
            {
                JObject jObject = jArray[i] as JObject;
                if(jObject == null)
                {
                    return;
                }

                result[i] = IJSAMObject<T>(jObject);

            });

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