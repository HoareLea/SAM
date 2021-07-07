using System.Runtime.InteropServices;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string FullName(object @object)
        {
            if (@object == null)
                return null;

            if (Marshal.IsComObject(@object))
                return COMObjectTypeName(@object);

            return @object.GetType().FullName;
        }
    }
}