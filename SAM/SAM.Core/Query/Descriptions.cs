using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<string> Descriptions<T>(params T[] excluded) where T: Enum
        {
            List<T> enums = Enums(excluded);
            if(enums == null)
            {
                return null;
            }

            List<string> result = new List<string>();
            for (int i = 0; i < enums.Count; i++)
            {
                result.Add(Description(enums[i]));
            }

            return result;
        }
    }
}