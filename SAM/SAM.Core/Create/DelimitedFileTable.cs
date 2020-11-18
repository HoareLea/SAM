using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Create
    {
        public static DelimitedFileTable DelimitedFileTable(DelimitedFileType delimitedFileType, string path)
        {
            if (delimitedFileType == DelimitedFileType.Undefined || string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return null;
            
            DelimitedFileTable result = null;
            using (DelimitedFileReader delimitedFileReader = new DelimitedFileReader(delimitedFileType, path))
                result = new DelimitedFileTable(delimitedFileReader);

            return result;
        }
    }
}