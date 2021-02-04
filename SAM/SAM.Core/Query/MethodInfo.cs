using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static MethodInfo MethodInfo(this IEnumerable<MethodInfo> methodInfos, string name, Type outputType, params Type[] inputTypes)
        {
            if (methodInfos == null || string.IsNullOrWhiteSpace(name))
                return null;
            
            foreach (MethodInfo methodInfo in methodInfos)
            {
                if (!methodInfo.Name.ToUpper().Equals(name))
                    continue;

                if (outputType != null)
                {
                    if (methodInfo.ReturnType != outputType)
                        continue;
                }

                if (inputTypes != null)
                {
                    ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                    if(parameterInfos != null)
                    {
                        if (parameterInfos.Length != inputTypes.Length)
                            continue;
                    }
                }

                return methodInfo;
            }

            return null;
        }
    }
}