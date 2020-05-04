using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static string ToJson(this IJSAMObject jSAMObject)
        {
            if (jSAMObject == null)
                return null;

            JObject jObject = jSAMObject.ToJObject();
            if (jObject == null)
                return null;

            return jObject.ToString();
        }

        public static string ToJson(this IEnumerable<IJSAMObject> jSAMObjects)
        {
            if (jSAMObjects == null)
                return null;

            JArray jArray = new JArray();
            foreach (IJSAMObject jSAMObject in jSAMObjects)
                jArray.Add(jSAMObject.ToJObject());

            return jArray.ToString();
        }
    }
}