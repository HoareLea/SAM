using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool IsValid(this Type type, Enum @enum)
        {
            if (type == null || @enum == null)
                return false;

            Type[] types = Attributes.Types.Get(@enum);
            if (types == null || types.Length == 0)
                return false;

            foreach(Type type_Temp in types)
            {
                if (type_Temp == null)
                    continue;

                if (type_Temp.IsAssignableFrom(type))
                    return true;
            }

            return false;
        }
    }
}