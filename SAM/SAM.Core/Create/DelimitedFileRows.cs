using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<DelimitedFileRow> DelimitedFileRows(DelimitedFileType delimitedFileType, string path)
        {
            if (delimitedFileType == DelimitedFileType.Undefined || string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return null;

            List<DelimitedFileRow> result = null;
            using (DelimitedFileReader delimitedFileReader = new DelimitedFileReader(delimitedFileType, path))
            {
                result = delimitedFileReader.Read();
            }

            return result;
        }
    }
}