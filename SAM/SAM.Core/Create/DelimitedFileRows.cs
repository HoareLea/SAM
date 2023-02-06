using System.Collections.Generic;

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

        public static List<DelimitedFileRow> DelimitedFileRows(object[,] values)
        {
            if(values == null)
            {
                return null;
            }

            int rowsStart = values.GetLowerBound(0);
            int rowsEnd = values.GetUpperBound(0);
            int rowsCount = rowsEnd - rowsStart + 1;
            if (rowsCount <= 0)
            {
                return null;
            }

            int columnsStart = values.GetLowerBound(1);
            int columnsEnd = values.GetUpperBound(1);
            int columnsCount = columnsEnd - columnsStart + 1;
            if (columnsCount <= 0)
            {
                return null;
            }

            List<DelimitedFileRow> result = new List<DelimitedFileRow>();
            for (int i = rowsStart; i <= rowsEnd; i++)
            {
                DelimitedFileRow delimitedFileRow = new DelimitedFileRow();
                for (int j = columnsStart; j <= columnsEnd; j++)
                {
                    delimitedFileRow.Add(values[i, j]?.ToString());
                }
                result.Add(delimitedFileRow);
            }

            return result;
        }
    }
}