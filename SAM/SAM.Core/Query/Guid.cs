using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Guid Guid(this JObject jObject)
        {
            if (jObject == null)
                return System.Guid.Empty;

            Guid guid = System.Guid.NewGuid();

            string guidString = jObject.Value<string>("Guid");
            if (!string.IsNullOrWhiteSpace(guidString))
            {
                Guid guid_Temp;
                if (System.Guid.TryParse(guidString, out guid_Temp))
                    guid = guid_Temp;
            }

            return guid;
        }
    }
}
