using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json;

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

        public static List<ParameterSet> ParameterSets(this JsonElement jsonElement)
        {
            if (jsonElement.ValueKind != JsonValueKind.Array)
                return null;


            List<ParameterSet> result = new List<ParameterSet>();

            foreach (object @object in jsonElement.EnumerateArray())
            {
                if(@object is JsonElement)
                    result.Add(new ParameterSet((JsonElement)@object));
            }

            return result;
        }
    }
}