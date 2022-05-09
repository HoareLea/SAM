using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Create
    {
        public static GuidCollection GuidCollection<T>(this IEnumerable<T> ts, string name = null, ParameterSet parameterSet = null, bool allowDuplicates = false) where T: IParameterizedSAMObject, ISAMObject
        {
            HashSet<Guid> guids = new HashSet<Guid>();
            GuidCollection result = new GuidCollection(name, parameterSet);
            if(ts != null)
            {
                foreach (T t in ts)
                {
                    if (t == null)
                        continue;

                    Guid guid = t.Guid;

                    if (!allowDuplicates)
                    {
                        if (guids.Contains(guid))
                            continue;

                        guids.Add(guid);
                    }

                    result.Add(guid);
                }
            }

            return result;
        }
    }
}