using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;

namespace SAM.Core
{
    public static partial class Create
    {
        public static IJSAMObject IJSAMObject(this JObject jObject)
        {
            if (jObject == null)
            {
                return null;
            }

            JSAMObjectWrapper jSAMObjectWrapper = new JSAMObjectWrapper(jObject);
            IJSAMObject jSAMObject = jSAMObjectWrapper.ToIJSAMObject();
            if (jSAMObject == null)
            {
                return jSAMObjectWrapper;
            }

            return jSAMObject;
        }

        public static T IJSAMObject<T>(this JObject jObject) where T : IJSAMObject
        {
            IJSAMObject jSAMObject = IJSAMObject(jObject);
            if (jSAMObject == null)
            {
                return default;
            }

            if (jSAMObject is T)
            {
                return (T)jSAMObject;
            }

            return default;
        }

        public static T IJSAMObject<T>(this string json) where T : IJSAMObject
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            JToken jToken = JToken.Parse(json);

            JObject jObject = jToken as JObject;
            if (jObject == null)
            {
                JArray jArray = jToken as JArray;
                if (jArray == null || jArray.Count == 0)
                {
                    return default;
                }

                jObject = jArray[0] as JObject;
            }

            if (jObject == null)
            {
                return default;
            }

            return IJSAMObject<T>(jObject);
        }

        public static IJSAMObject IJSAMObject(this string json)
        {
            return IJSAMObject<IJSAMObject>(json);
        }

        public static IJSAMObject IJSAMObject(this ZipArchiveEntry zipArchiveEntry)
        {
            if (zipArchiveEntry == null)
            {
                return null;
            }

            IJSAMObject result = null;
            using (StreamReader streamReader = new StreamReader(zipArchiveEntry.Open()))
            {
                result = IJSAMObject(streamReader.ReadToEnd());
            }

            return result;
        }

        public static T IJSAMObject<T>(this ZipArchiveEntry zipArchiveEntry) where T : IJSAMObject
        {
            IJSAMObject jSAMObject = IJSAMObject(zipArchiveEntry);
            if (jSAMObject is T)
            {
                return (T)jSAMObject;
            }

            return default;
        }
    }
}
