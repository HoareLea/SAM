using System;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool TryInvokeMethod(this object @object, string methodName, out object result, params object[] parameters)
        {
            result = null;

            if(@object == null || string.IsNullOrWhiteSpace(methodName))
            {
                return false;
            }

            foreach (MethodInfo methodInfo in @object.GetType().GetMethods())
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