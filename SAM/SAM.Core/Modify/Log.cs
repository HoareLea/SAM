using System;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Log(this string path, string format, params object[] values)
        {
            if(format == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string directory = System.IO.Path.GetDirectoryName(path);
            if(string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            if(!Create.Directory(directory))
            {
                return false;
            }

            LogRecord logRecord = new LogRecord(format, values);

            try
            {
                logRecord.Write(path);
            }
            catch(Exception exception)
            {
                return false;
            }

            return true;
        }
    }
}