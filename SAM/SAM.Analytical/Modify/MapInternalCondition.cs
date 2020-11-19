using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static InternalCondition MapInternalCondition(this Space space, InternalConditionLibrary internalConditionLibrary, TextMap textMap, bool overrideNotFound = false, InternalCondition internalCondition_Default = null)
        {
            if (space == null || internalConditionLibrary == null || textMap == null)
                return null;

            string name = space.Name;
            if (string.IsNullOrWhiteSpace(name))
                return null;

            InternalCondition result = null;

            HashSet<string> names_InternalCondition = textMap.GetSortedKeys(name);
            if (names_InternalCondition != null && names_InternalCondition.Count != 0)
            {
                List<InternalCondition> internalConditions = internalConditionLibrary.GetInternalConditions(names_InternalCondition.First());
                if (internalConditions != null && internalConditions.Count != 0)
                    result = internalConditions[0];
            }

            if (!overrideNotFound && result == null)
                return null;

            if (result == null)
                result = internalCondition_Default;

            space.InternalCondition = result;
            return result;
        }
    }
}