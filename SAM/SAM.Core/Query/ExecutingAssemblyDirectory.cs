using System;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string ExecutingAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly()?.CodeBase;
            if (string.IsNullOrEmpty(codeBase))
                return null;

            UriBuilder uriBuilder = new UriBuilder(codeBase);
            
            string path = uriBuilder.Path;
            if (string.IsNullOrEmpty(path))
                return null;

            path = Uri.UnescapeDataString(path);

            return System.IO.Path.GetDirectoryName(path);
        }
    }
}