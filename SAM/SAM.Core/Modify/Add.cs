using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Add(this List<ParameterSet> parameterSets, ParameterSet parameterSet)
        {
            if (parameterSets == null || parameterSet == null)
                return false;

            ParameterSet parameterSet_Existing = parameterSets.Find(x => x.Guid.Equals(parameterSet.Guid));
            if (parameterSet_Existing == null)
            {
                parameterSets.Add(parameterSet);
                return true;
            }

            return parameterSet_Existing.Copy(parameterSet);
        }

        public static LogRecord Add(this Log log, string format, params object[] values)
        {
            if (log == null || format == null)
                return null;

            return log.Add(format, values);
        }

        public static LogRecord Add(this Log log, string format, LogRecordType logRecordType, params object[] values)
        {
            if (log == null || format == null)
                return null;

            return log.Add(format, logRecordType, values);
        }


    }
}