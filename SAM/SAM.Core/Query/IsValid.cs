using System;
using System.Net;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool IsValid(this Type type, Enum @enum)
        {
            if (type == null || @enum == null)
                return false;

            Type[] types = Attributes.AssociatedTypes.Get(@enum);
            if (types == null || types.Length == 0)
                return false;

            foreach(Type type_Temp in types)
            {
                if (type_Temp == null)
                    continue;

                if (type_Temp.IsAssignableFrom(type))
                    return true;
            }

            return false;
        }

        public static bool IsValid(this Uri uri)
        {
            if (uri == null)
                return false;
            
            HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
            if (httpWebRequest == null)
                return false;

            httpWebRequest.AllowAutoRedirect = false;

            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            if (httpWebResponse == null)
                return false;

            return httpWebResponse.StatusCode == HttpStatusCode.OK;
        }
    }
}