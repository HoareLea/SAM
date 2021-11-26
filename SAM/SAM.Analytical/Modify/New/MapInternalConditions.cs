using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<InternalCondition> MapInternalConditions(this BuildingModel buildingModel, InternalConditionLibrary internalConditionLibrary, TextMap textMap, bool overrideNotFound = false, InternalCondition internalCondition_Default = null)
        {
            if (buildingModel == null || internalConditionLibrary == null || textMap == null)
                return null;

            List<Space> spaces = buildingModel.GetSpaces();
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

                buildingModel.Add(space_Temp);
                result.Add(internalCondition);
            }

            return result;
        }
    }
}