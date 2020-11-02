﻿using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static List<string[]> ToList(this string text, DelimitedFileType delimitedFileType = DelimitedFileType.Csv)
        {
            return ToList(text, Query.Separator(delimitedFileType));
        }

        public static List<string[]> ToList(this string text, char separator = ',')
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string[] lines = text.Split('\n');

            DelimitedFileTable delimitedFileTable = new DelimitedFileTable(new DelimitedFileReader(separator, lines));

            List<string> columnNames = delimitedFileTable.GetColumnNames();

            List<string[]> result = new List<string[]>();
            foreach (string columnName in columnNames)
            {
                string[] values = new string[delimitedFileTable.RowCount];
                for (int i = 0; i < delimitedFileTable.RowCount; i++)
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