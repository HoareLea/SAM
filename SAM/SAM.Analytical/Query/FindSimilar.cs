using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<TParameterizedSAMObject> FindSimilar<TParameterizedSAMObject>(TParameterizedSAMObject parameterizedSAMObject, IEnumerable<TParameterizedSAMObject> parameterizedSAMObjects, IEnumerable<string> excludedParameterNames = null) where TParameterizedSAMObject : IParameterizedSAMObject
        {
            if (parameterizedSAMObjects == null)
            {
                return null;
            }

            List<string> names = parameterizedSAMObject?.UserFriendlyNames();
            if (names == null || names.Count == 0)
            {
                return null;
            }

            List<TParameterizedSAMObject> result = new List<TParameterizedSAMObject>();
            if (parameterizedSAMObjects.Count() == 0)
            {
                return result;
            }

            if (excludedParameterNames != null)
            {
                names.RemoveAll(x => excludedParameterNames.Contains(x));
            }

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string name in names)
            {
                if (parameterizedSAMObject.TryGetValue(name, out object value, true))
                {
                    dictionary[name] = value?.ToString();
                }
            }

            foreach (TParameterizedSAMObject parameterizedSAMObject_Temp in parameterizedSAMObjects)
            {
                List<string> names_Temp = parameterizedSAMObject_Temp?.UserFriendlyNames();
                if (names_Temp == null || names_Temp.Count == 0)
                {
                    continue;
                }

                if (excludedParameterNames != null)
                {
                    names_Temp.RemoveAll(x => excludedParameterNames.Contains(x));
                }

                if (names.Count != names_Temp.Count)
                {
                    continue;
                }

                bool add = true;

                foreach (string name_Temp in names_Temp)
                {
                    if (!dictionary.TryGetValue(name_Temp, out string value))
                    {
                        add = false;
                        break;
                    }

                    if (!parameterizedSAMObject.TryGetValue(name_Temp, out object value_Temp, true))
                    {
                        add = false;
                        break;
                    }

                    if (value != value_Temp?.ToString())
                    {
                        add = false;
                        break;
                    }
                }

                if (!add)
                {
                    continue;
                }

                result.Add(parameterizedSAMObject_Temp);
            }

            return result;
        }
    }
}