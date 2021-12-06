using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static JArray JArray(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            if (!TryGetJToken(json, out JToken jToken) || jToken == null)
                return null;

            if (jToken is JObject)
                return new JArray() { jToken };
            else if (jToken is JArray)
                return (JArray)jToken;

            return null;
        }

        public static JArray JArray<T>(this T[,] values)
        {
            if(values == null)
            {
                return null;
            }
            
            JArray result = new JArray();
            for (int i = 0; i < values.GetLength(0); i++)
            {
                JArray jArray_Temp = new JArray();
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    jArray_Temp.Add(values[i, j]);
                }
                result.Add(jArray_Temp);
            }

            return result;
        }
    }
}