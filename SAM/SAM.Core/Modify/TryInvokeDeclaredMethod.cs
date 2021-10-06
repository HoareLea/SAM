using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool TryInvokeDeclaredMethod(dynamic @object, string methodName, out object result, params object[] parameters)
        {

            result = null;

            if(@object == null || string.IsNullOrWhiteSpace(methodName))
            {
                return false;
            }

            IEnumerable<MethodInfo> methodInfos = null;
            try
            {
                methodInfos = @object.GetType().DeclaredMethods;
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