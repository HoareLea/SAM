using Newtonsoft.Json.Linq; // Required for working with JSON
using System.Collections.Generic; // Required for using List<>
using System.Linq; // Required for using Enumerable.Repeat() and Enumerable.ToList()
using System.Threading.Tasks; // Required for using Parallel.For()

namespace SAM.Core
{
    public static partial class Create
    {
        // This method converts a JArray into a List of IJSAMObjects of type T
        public static List<T> IJSAMObjects<T>(this JArray jArray) where T : IJSAMObject
        {
            if (jArray == null)
            {
                return null;
            }

            // Initialize a List of type T with default values
            List<T> result = Enumerable.Repeat<T>(default, jArray.Count).ToList();

            // Process each element of the JArray in parallel
            Parallel.For(0, jArray.Count, (int i) =>
            {
                // Convert the current element to a JObject
                JObject jObject = jArray[i] as JObject;

                // Skip if the current element is not a JObject
                if (jObject == null)
                {
                    return;
                }

                // Convert the current element to an IJSAMObject of type T and store it in the result List
                result[i] = IJSAMObject<T>(jObject);

            });

            // Return the List of IJSAMObjects of type T
            return result;
        }

        // This method converts a JSON string into a List of IJSAMObjects of type T
        public static List<T> IJSAMObjects<T>(this string json) where T : IJSAMObject
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            // Convert the JSON string to a JArray
            JArray jArray = Query.JArray(json);

            // Return null if the JArray is null
            if (jArray == null)
            {
                return null;
            }

            // Convert the JArray to a List of IJSAMObjects of type T using the IJSAMObjects<JObject> method
            return IJSAMObjects<T>(jArray);
        }
    }
}
