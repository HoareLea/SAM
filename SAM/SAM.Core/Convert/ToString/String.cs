using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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
    }
}