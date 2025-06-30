using System.Diagnostics;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Process StartProcess(this string path, string arguments = null)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true,
                Verb = "open",
                Arguments = arguments
            };

            return Process.Start(processStartInfo);
        }
    }
}