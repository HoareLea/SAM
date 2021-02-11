using System.Collections.Generic;
using System.Linq;

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

        public static DelimitedFileTable DelimitedFileTable(IEnumerable<SAMObject> sAMObjects, IEnumerable<string> parameters)
        {
            if (parameters == null || sAMObjects == null)
                return null;

            int rowCount = sAMObjects.Count() + 1;
            int columnCount = parameters.Count();

            object[,] data = new object[rowCount, columnCount];

            for (int i = 0; i < columnCount; i++)
                data[0, i] = parameters.ElementAt(i);

            for (int i = 1; i < rowCount; i++)
            {
                SAMObject sAMObject = sAMObjects.ElementAt(i - 1);
                if (sAMObject == null)
                    continue;

                for (int j = 0; j < columnCount; j++)
                {
                    string parameter = parameters.ElementAt(j);
                    if (parameter == null)
                        continue;

                    if (!Query.TryGetValue(sAMObject, parameter, out object value))
                        continue;

                    data[i, j] = value;
                }
            }

            return new DelimitedFileTable(data);
        }
    }
}