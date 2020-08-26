using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string LatestVersion()
        {
            string url = @"https://api.github.com/repos/HoareLea/SAM_Deploy/releases/latest";

            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(url);
            if (httpWebRequest == null)
                return null;

            string userAgent = "SAM";

            string currentVersion = CurrentVersion();

            if (!string.IsNullOrWhiteSpace(currentVersion))
                userAgent = string.Format("{0}({1})", userAgent, currentVersion);

            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.UserAgent = userAgent;

            string json = null;
            
            using (WebResponse webResponse = httpWebRequest.GetResponse())
            {
                Stream stream = webResponse.GetResponseStream();
                if(stream != null)
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        json = streamReader.ReadToEnd();
                    }
                }   
            }

            if (string.IsNullOrWhiteSpace(json))
                return null;

            JObject jObject = JObject.Parse(json);
            if (jObject == null)
                return null;

            return jObject.Value<string>("tag_name");
        }
    }
}