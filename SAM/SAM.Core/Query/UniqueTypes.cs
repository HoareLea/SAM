using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static HashSet<Type> UniqueTypes(this IEnumerable<Attributes.AssociatedTypes> parameterTypes)
        {
            if (parameterTypes == null)
                return null;

            HashSet<Type> result = new HashSet<Type>();
            foreach(Attributes.AssociatedTypes parameterTypes_Temp in parameterTypes)
            {
                if (parameterTypes_Temp == null)
                    continue;

                Type[] types = parameterTypes_Temp.Types;
                if (types == null)
                    continue;

                foreach (Type type in types)
                    result.Add(type);
            }

            return result;
        }
    }
}