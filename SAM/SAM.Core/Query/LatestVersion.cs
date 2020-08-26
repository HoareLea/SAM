using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string LatestVersion()
        {
            string url = @"https://api.github.com/repos/HoareLea/SAM_Deploy/releases/latest";

            HttpWebRequest httpWebRequest = null;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            catch
            {
                return null;
            }

            if (httpWebRequest == null)
                return null;

            string json = null;

            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = httpWebResponse.GetResponseStream();
                    StreamReader streamReader = null;

                    if (string.IsNullOrWhiteSpace(httpWebResponse.CharacterSet))
                        streamReader = new StreamReader(stream);
                    else
                        streamReader = new StreamReader(stream, Encoding.GetEncoding(httpWebResponse.CharacterSet));

                    json = streamReader.ReadToEnd();

                    if (streamReader != null)
                        streamReader.Close();

                    httpWebResponse.Close();
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