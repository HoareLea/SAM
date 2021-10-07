
namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryInvokeDeclaredMethod<T>(dynamic @object, string methodName, out T result, params object[] parameters)
        {
            return TryInvokeMethod<T>(@object, @object?.GetType().DeclaredMethods, methodName, out result, parameters);
        }
    }
}