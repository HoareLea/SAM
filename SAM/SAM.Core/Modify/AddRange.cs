using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static List<LogRecord> AddRange(this Log log, IEnumerable<LogRecord> logRecords)
        {
            if (log == null || logRecords == null)
                return null;

            return log.AddRange(logRecords);
        }
    }
}