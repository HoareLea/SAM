using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<ParameterSet> Clone(this List<ParameterSet> parameterSets)
        {
            if (parameterSets == null)
                return null;

            List<ParameterSet> result = new List<ParameterSet>();
            foreach (ParameterSet parameterSet in parameterSets)
                result.Add(parameterSet.Clone());

            return result;
        }

        public static T Clone<T>(this T jSAMObject) where T: IJSAMObject
        {
            Type type = jSAMObject?.GetType();
            if (type == null)
                return default;

            MethodInfo[] methodInfos = type.GetMethods();
            if(methodInfos != null && methodInfos.Length != 0)
            {
                foreach(MethodInfo methodInfo in methodInfos)
                {
                    if (!methodInfo.Name.Equals("Clone"))
                        continue;
                    
                    if (!methodInfo.ReturnType.IsAssignableFrom(type))
                        continue;
                    
                    ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos == null || parameterInfos.Length == 0)
                    {
                        object result = methodInfo.Invoke(jSAMObject, new object[0]);
                        if (result is T)
                            return (T)result;
                    }

                }
            }

            ConstructorInfo constructorInfo_Empty = null;
            ConstructorInfo constructorInfo_Type = null;
            ConstructorInfo constructorInfo_Type_AssignableFrom = null;

            foreach (ConstructorInfo constructorInfo in type.GetConstructors())
            {
                ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
                if(parameterInfos == null || parameterInfos.Length == 0)
                {
                    constructorInfo_Empty = constructorInfo;
                    continue;
                }

                if (parameterInfos.Length != 1)
                    continue;

                ParameterInfo parameterInfo = parameterInfos.First();

                if (!parameterInfo.ParameterType.Equals(type))
                {
                    if (parameterInfo.ParameterType.IsAssignableFrom(type))
                        constructorInfo_Type_AssignableFrom = constructorInfo;

                    continue;
                }
                    

                constructorInfo_Type = constructorInfo;
                break;
            }

            if (constructorInfo_Type != null)
            {
                object result = constructorInfo_Type.Invoke(new object[] { jSAMObject });
                if (result is T)
                    return (T)result;
            }

            if (constructorInfo_Type_AssignableFrom != null)
            {
                object result = constructorInfo_Type_AssignableFrom.Invoke(new object[] { jSAMObject });
                if (result is T)
                    return (T)result;
            }

            if (constructorInfo_Empty != null)
            {
                object result = constructorInfo_Empty.Invoke(new object[0]);
                if (result is T)
                    return (T)result;
            }

            return default;

        }
    }
}