using System.Runtime.InteropServices;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string TypeName(this object @object)
        {
            if(@object == null)
            {
                return null;
            }

            return Marshal.IsComObject(@object) ? COMObjectTypeName(@object) : @object.GetType().Name;
        }
    }
}