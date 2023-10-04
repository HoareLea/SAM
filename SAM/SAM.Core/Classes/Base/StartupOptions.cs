using System.Collections.Generic;

namespace SAM.Core
{
    public class StartupOptions
    {
        public string Path { get; set; }
        public bool TemporaryFile { get; set; }

        public StartupOptions()
        {

        }

        public override string ToString()
        {
            List<string> values = new List<string>();
            if (!string.IsNullOrWhiteSpace(Path))
            {
                values.Add(string.Format("/{0}=\"{1}\"", StartupArgument.Path, Path));
            }

            if (TemporaryFile)
            {
                values.Add(string.Format("/{0}", StartupArgument.TemporaryFile.ToString()));
            }

            return string.Join(" ", values);
        }
    }
}
