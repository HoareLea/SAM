using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetInternalCondition(this Space space, InternalConditionLibrary internalConditionLibrary, TextMap textMap, out InternalCondition internalCondition)
        {
            internalCondition = null;

            if (space == null || internalConditionLibrary == null || textMap == null)
                return false;

            string name = space.Name;
            if (string.IsNullOrEmpty(name))
                return false;

            HashSet<string> names_InternalCondition = textMap.GetSortedKeys(name);
            if (names_InternalCondition == null || names_InternalCondition.Count == 0)
                return false;

            List<InternalCondition> internalConditions = internalConditionLibrary.GetInternalConditions(names_InternalCondition.First());
            if (internalConditions == null && internalConditions.Count == 0)
                return false;

            internalCondition = internalConditions[0];
            return true;
        }
    }
}