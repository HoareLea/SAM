using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool TryGetInternalConditions(this Space space, InternalConditionLibrary internalConditionLibrary, TextMap textMap, out List<InternalCondition> internalConditions)
        {
            internalConditions = null;

            if (space == null || internalConditionLibrary == null || textMap == null)
            {
                return false;
            }

            string name = space.Name;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            HashSet<string> names_InternalCondition = textMap.GetSortedKeys(name);
            if (names_InternalCondition == null || names_InternalCondition.Count == 0)
            {
                return false;
            }

            internalConditions = new List<InternalCondition>();
            foreach(string name_InternalCondition in names_InternalCondition)
            {
                List<InternalCondition> internalConditions_Temp = internalConditionLibrary.GetInternalConditions(name_InternalCondition);
                if (internalConditions_Temp == null || internalConditions_Temp.Count == 0)
                {
                    continue;
                }

                internalConditions.AddRange(internalConditions_Temp);
            }

            return true;
        }
    }
}