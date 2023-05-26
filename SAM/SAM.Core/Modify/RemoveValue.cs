using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool RemoveValue(this ParameterizedSAMObject parameterizedSAMObject, string name, Assembly defaultAssembly, bool findAny = true)
        {
            if (parameterizedSAMObject == null)
                return false;

            ParameterSet parameterSet = parameterizedSAMObject.GetParameterSet(defaultAssembly);
            if (!findAny && parameterSet == null)
                return false;

            if (parameterSet != null && parameterSet.Contains(name))
                return parameterSet.Remove(name);

            if (!findAny)
                return false;

            List<ParameterSet> parameterSets = parameterizedSAMObject.GetParameterSets();
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