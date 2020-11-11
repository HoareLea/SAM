using Newtonsoft.Json.Linq;
using System;
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
            if (parameterSet == null)
                return false;

            if (parameterSet.Contains(name))
                return parameterSet.Remove(name);

            if (!findAny)
                return false;

            foreach(ParameterSet parameterSet_Temp in sAMObject.GetParamaterSets())
                if (parameterSet_Temp.Contains(name))
                    return parameterSet_Temp.Remove(name);

            return false;
        }
    }
}