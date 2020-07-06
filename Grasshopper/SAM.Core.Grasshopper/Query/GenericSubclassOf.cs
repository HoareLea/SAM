using System;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static bool GenericSubclassOf(this Type type, Type baseGenericType)
        {
            for (; type != typeof(object); type = type.BaseType)
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (baseGenericType == cur)
                    return true;
            }

            return false;
        }
    }
}