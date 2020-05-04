using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static List<T> ToSAM<T>(string pathOrJson) where T : IJSAMObject
        {
            if (string.IsNullOrWhiteSpace(pathOrJson))
                return null;

            JArray jArray = Query.JArray(pathOrJson);
            if (jArray == null)
                return null;

            return Create.IJSAMObjects<T>(jArray);
        }

        public static List<IJSAMObject> ToSAM(string pathOrJson)
        {
            return ToSAM<IJSAMObject>(pathOrJson);
        }
    }
}