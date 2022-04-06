using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool RemoveValue(this SAMObject sAMObject, string name, Assembly defaultAssembly, bool findAny = true)
        {
            if (sAMObject == null)
                return false;

            ParameterSet parameterSet = sAMObject.GetParameterSet(defaultAssembly);
            if (!findAny && parameterSet == null)
                return false;

            if (parameterSet != null && parameterSet.Contains(name))
                return parameterSet.Remove(name);

            if (!findAny)
                return false;

            List<ParameterSet> parameterSets = sAMObject.GetParamaterSets();
            if(parameterSets != null)
            {
                foreach (ParameterSet parameterSet_Temp in parameterSets)
                {
                    if (parameterSet_Temp.Contains(name))
                    {
                        return parameterSet_Temp.Remove(name);
                    }
                }
            }

            return false;
        }
    }
}