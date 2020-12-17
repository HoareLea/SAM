using System.Text.Json;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static JsonElement? ToJsonElement(this System.Dynamic.ExpandoObject expandoObject)
        {
            if (expandoObject == null)
                return null;

            JsonDocument jsonDocument = expandoObject.ToJsonDocument();
            if (jsonDocument == null)
                return null;

            return jsonDocument.RootElement;
        }
    }
}