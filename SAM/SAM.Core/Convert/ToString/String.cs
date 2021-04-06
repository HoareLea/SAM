using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static string ToString(this IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return null;

            JObject jObject = jSAMObject.ToJObject();
            if (jObject == null)
                return null;

            return jObject.ToString();
        }

        public static string ToString(this IEnumerable<IJSAMObject> jSAMObjects)
        {
            if (jSAMObjects == null)
                return null;

            JArray jArray = new JArray();
            foreach (IJSAMObject jSAMObject in jSAMObjects)
                jArray.Add(jSAMObject.ToJObject());

            return jArray.ToString();
        }

        public static string ToString<T>(IEnumerable<T> sAMObjects, string path) where T : IJSAMObject
        {
            if (sAMObjects == null)
                return null;

            string @string = ToString(sAMObjects.Cast<IJSAMObject>());

            System.IO.File.WriteAllText(path, @string);

            return @string;
        }

        public static string ToString(Color color)
        {
            return color.Name;
        }

        public static string ToString(JsonDocument jsonDocument)
        {
            if (jsonDocument == null)
                return null;

            string result = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(memoryStream, new JsonWriterOptions { Indented = true });
                jsonDocument.WriteTo(utf8JsonWriter);
                utf8JsonWriter.Flush();
                result =  Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            return result;
        }

        public static string ToString(this System.Dynamic.ExpandoObject expandoObject)
        {
            if (expandoObject == null)
                return null;

            return JsonSerializer.Serialize(expandoObject);

            //return ToString(expandoObject.ToJsonDocument());
        }
    }
}