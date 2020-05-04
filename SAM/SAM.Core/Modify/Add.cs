using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Add(this List<ParameterSet> parameterSets, ParameterSet parameterSet)
        {
            if (parameterSets == null || parameterSet == null)
                return false;

            ParameterSet parameterSet_Existing = parameterSets.Find(x => x.Guid.Equals(parameterSet.Guid));
            if (parameterSet_Existing == null)
            {
                parameterSets.Add(parameterSet);
                return true;
            }

            return parameterSet_Existing.Copy(parameterSet);
        }
    }
}