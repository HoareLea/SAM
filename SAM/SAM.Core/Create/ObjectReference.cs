using System;

namespace SAM.Core
{
    public static partial class Create
    {
        public static ObjectReference ObjectReference<T>(this T @object, Func<T, Reference> func = null)
        {
            if(@object == null)
            {
                return null;
            }

            return new ObjectReference(@object.GetType(), func?.Invoke(@object));
        }
    }
}