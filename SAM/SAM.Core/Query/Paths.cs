using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<string> Paths(this string directory, string fileName, params string[] extensions)
        {
            if(string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName) || extensions == null || extensions.Length == 0)
            {
                return null;
            }

            List<string> result = new List<string>();
            foreach(string extension in extensions)
            {
                string path = System.IO.Path.Combine(directory, string.Format("{0}.{1}", fileName, extension));
                result.Add(path);
            }

            return result;
        }
    }
}