using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static List<string[]> ToList(this string text, DelimitedFileType delimitedFileType = DelimitedFileType.Csv)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string[] lines = text.Split('\n');

            DelimitedFileTable delimitedFileTable = new DelimitedFileTable(new DelimitedFileReader(Query.Separator(delimitedFileType), lines));

            List<string> columnNames = delimitedFileTable.GetColumnNames();

            List<string[]> result = new List<string[]>();
            foreach (string columnName in columnNames)
            {
                string[] values = new string[delimitedFileTable.Count];
                for (int i = 0; i < delimitedFileTable.Count; i++)
                {
                    object value = delimitedFileTable[i, columnName];
                    if (value != null)
                        values[i] = value.ToString();
                }
                result.Add(values);
            }
            return result;
        }
    }
}