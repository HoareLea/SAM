using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryInvokeRuntimeMethod(object @object, string methodName, out object result, params object[] parameters)
        {

            result = null;

            if(@object == null || string.IsNullOrWhiteSpace(methodName))
            {
                return false;
            }

            IEnumerable<MethodInfo> methodInfos = null;
            try
            {
                methodInfos = @object.GetType().GetRuntimeMethods();
            }
            catch
            {
                return false;
            }

            if(methodInfos == null)
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
                    result = methodInfo.Invoke(@object, parameters);
                    return true;
                }
                catch(Exception exception)
                {
                    result = null;
                }
            }

            return false;
        }
    }
}