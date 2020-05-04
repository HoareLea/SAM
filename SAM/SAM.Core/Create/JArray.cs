using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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

        public static JArray JArray<T>(this IEnumerable<T> jSAMObjects) where T : IJSAMObject
        {
            if (jSAMObjects == null)
                return null;

            JArray jArray = new JArray();
            foreach (T t in jSAMObjects)
                jArray.Add(t.ToJObject());

            return jArray;
        }
    }
}