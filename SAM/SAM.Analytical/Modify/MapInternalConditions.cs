using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<InternalCondition> MapInternalConditions(this IEnumerable<Space> spaces, InternalConditionLibrary internalConditionLibrary, TextMap textMap, bool overrideNotFound = false, InternalCondition internalCondition_Default = null)
        {
            if (spaces == null || internalConditionLibrary == null || textMap == null)
                return null;

            List<InternalCondition> result = new List<InternalCondition>();
            foreach (Space space in spaces)
            {
                InternalCondition internalCondition;
                if (space == null || (!space.TryGetInternalCondition(internalConditionLibrary, textMap, out internalCondition) && !overrideNotFound))
                {
                    result.Add(null);
                    continue;
                }

                if (internalCondition == null)
                    internalCondition = internalCondition_Default;

                space.InternalCondition = internalCondition;
                result.Add(internalCondition);
            }

            return result;
        }

        public static List<InternalCondition> MapInternalConditions(this AdjacencyCluster adjacencyCluster, InternalConditionLibrary internalConditionLibrary, TextMap textMap, bool overrideNotFound = false, InternalCondition internalCondition_Default = null)
        {
            if (adjacencyCluster == null || internalConditionLibrary == null || textMap == null)
                return null;

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null)
                return null;

            List<InternalCondition> result = new List<InternalCondition>();
            foreach (Space space in spaces)
            {
                InternalCondition internalCondition;
                if (space == null || (!space.TryGetInternalCondition(internalConditionLibrary, textMap, out internalCondition) && !overrideNotFound))
                {
                    result.Add(null);
                    continue;
                }

                if (internalCondition == null)
                    internalCondition = internalCondition_Default;

                Space space_Temp = new Space(space);
                space_Temp.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space_Temp);
                result.Add(internalCondition);
            }

            return result;
        }
    }
}