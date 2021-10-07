using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryInvokeMethod<T>(this object @object, string methodName, out T result, params object[] parameters)
        {
            return TryInvokeMethod<T>(@object, @object?.GetType().GetMethods(), methodName, out result, parameters);
        }

        public static bool TryInvokeMethod<T>(this object @object, IEnumerable<MethodInfo> methodInfos, string methodName, out T result, params object[] parameters)
        {
            result = default;

            if (@object == null || methodInfos == null || string.IsNullOrWhiteSpace(methodName))
            {
                return false;
            }

            foreach (MethodInfo methodInfo in methodInfos)
            {
                if (methodInfo?.Name != methodName)
                {
                    continue;
                }

                try
                {
                    object object_Result = methodInfo.Invoke(@object, parameters);
                    if (object_Result is T)
                    {
                        result = (T)(object)(object_Result);
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    result = default;
                }
            }

            return false;
        }
    }
}