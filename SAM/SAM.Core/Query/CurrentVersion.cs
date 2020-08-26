namespace SAM.Core
{
    public static partial class Query
    {
        public static string CurrentVersion()
        {
            string directory = ExecutingAssemblyDirectory();
            if (string.IsNullOrWhiteSpace(directory))
                return null;

            string path = System.IO.Path.Combine(directory, "version");
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return null;

            string[] lines = System.IO.File.ReadAllLines(path);
            if (lines == null || lines.Length == 0)
                return null;

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    return line.Trim();
            }

            return null;
        }
    }
}