using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<ParameterSet> ParameterSets(this JArray jArray)
        {
            if (jArray == null)
                return null;

            List<ParameterSet> result = new List<ParameterSet>();

            foreach (JObject jObject in jArray)
                result.Add(new ParameterSet(jObject));

            return result;
        }
    }
}
