using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<Type> ValidTypes(this Attributes.AssociatedTypes parameterTypes, IEnumerable<Type> types)
        {
            if (parameterTypes == null || types == null)
                return null;

            HashSet<Type> result = new HashSet<Type>();
            foreach (Type type in types)
                if (parameterTypes.IsValid(type))
                    result.Add(type);

            return result.ToList();
        }
    }
}