using System.Collections.Generic;
using System.IO;

namespace SAM.Core
{
    public static partial class Create
    {
        public static bool Directory(this string directory)
        {
            if(string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            string directory_Temp = directory;
            if (!string.IsNullOrWhiteSpace(Path.GetExtension(directory_Temp)))
            {
                directory_Temp = Path.GetDirectoryName(directory_Temp);
            }

            List<string> directories = new List<string>();
            directories.Add(directory_Temp);

            directory_Temp = Path.GetDirectoryName(directory_Temp);
            while(!string.IsNullOrWhiteSpace(directory_Temp))
            {
                directories.Add(directory_Temp);
                directory_Temp = Path.GetDirectoryName(directory_Temp);
            }

            if(directories.Count < 2)
            {
                return false;
            }

            for(int i = directories.Count - 2; i >= 0; i--)
            {
                directory_Temp = directories[i];

                if(System.IO.Directory.Exists(directory_Temp))
                {
                    continue;
                }

                try
                {
                    DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(directory_Temp);
                    if(directoryInfo == null)
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}