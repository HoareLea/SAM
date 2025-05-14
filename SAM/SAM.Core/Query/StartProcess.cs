using System.Diagnostics;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Process StartProcess(this string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true,
                Verb = "open"
            };

            return Process.Start(processStartInfo);
        }
    }
}