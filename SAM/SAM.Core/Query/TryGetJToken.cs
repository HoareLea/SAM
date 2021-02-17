using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryGetJToken(this string @string, out JToken jToken)
        {
            jToken = null;
            
            if (string.IsNullOrWhiteSpace(@string))
                return false;

            try
            {
                jToken = JToken.Parse(@string);
                return true;
            }
            catch
            {

            }

            return false;
        }
    }
}