using System.Text.Json;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static JsonDocument ToJsonDocument(this System.Dynamic.ExpandoObject expandoObject)
        {
            if (expandoObject == null)
                return null;
            
            JsonDocument result = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(expandoObject));

            return result;
        }
    }
}