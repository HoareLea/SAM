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
            foreach(Space space in spaces)
                result.Add(space.MapInternalCondition(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default));

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
                InternalCondition internalCondition = space.MapInternalCondition(internalConditionLibrary, textMap, overrideNotFound, internalCondition_Default);
                if (internalCondition != space.InternalCondition)
                {
                    adjacencyCluster.AddObject(space);
                    result.Add(internalCondition);
                    continue;
                }

                result.Add(null);
            }

            return result;
        }
    }
}