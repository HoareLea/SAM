using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static List<Enum> Copy<T>(this T object_Source, T object_Destination, params Enum[] enums) where T: SAMObject
        {
            if (object_Source == null || object_Destination == null || enums == null)
                return null;

            List<Enum> result = new List<Enum>();
            foreach(Enum @enum in enums)
            {
                if (!object_Source.TryGetValue(@enum, out object value))
                    continue;

                if (!object_Destination.SetValue(@enum, value))
                    continue;

                result.Add(@enum);
            }

            return result;
        }
    }
}