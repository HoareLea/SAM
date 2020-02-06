using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static JArray JArray(this IEnumerable<ParameterSet> parameterSets)
        {
            if (parameterSets == null)
                return null;

            JArray jArray = new JArray();
            foreach (ParameterSet parameterSet in parameterSets)
                jArray.Add(parameterSet.ToJObject());

            return jArray;
        }

        public static JArray JArray<T>(this IEnumerable<T> sAMObjects) where T : IJSAMObject
        {
            if (sAMObjects == null)
                return null;

            JArray jArray = new JArray();
            foreach (T t in sAMObjects)
                jArray.Add(t.ToJObject());

            return jArray;
        }
    }
}
