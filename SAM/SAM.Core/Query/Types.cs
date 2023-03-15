using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<Type> Types(this IRelationFilter relationFilter)
        {
            List<Type> result = new List<Type>();
            Types(relationFilter, result);

            return result;
        }

        public static void Types(this IRelationFilter relationFilter, List<Type> types)
        {
            if (types == null)
            {
                types = new List<Type>();
            }

            types.Add(relationFilter.GetType());

            IRelationFilter relationFilter_Temp = relationFilter.Filter as IRelationFilter;
            if (relationFilter_Temp == null)
            {
                return;
            }

            Types(relationFilter_Temp, types);
        }
    }
}