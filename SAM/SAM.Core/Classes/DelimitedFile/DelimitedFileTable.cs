using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class DelimitedFileTable : IDisposable, IEnumerable<object[]>
    {
        private bool disposed = false;

        private string[] names;
        private List<object[]> header;
        private List<object[]> values;

        private DelimitedFileTable()
        {
        }

        public DelimitedFileTable(DelimitedFileTable delimitedFileTable)
        {
            if (delimitedFileTable == null)
                return;
            if (delimitedFileTable.names != null)
                names = new List<string>(delimitedFileTable.names).ToArray();

            if (delimitedFileTable.header != null)
            {
                header = new List<object[]>();
                foreach (object[] aObjects in delimitedFileTable.header)
                {
                    if (aObjects == null)
                        header.Add(null);
                    else
                        header.Add(new List<object>(aObjects).ToArray());
                }
            }

            if (delimitedFileTable.values != null)
            {
                values = new List<object[]>();
                foreach (object[] aObjects in delimitedFileTable.values)
                {
                    if (aObjects == null)
                        values.Add(null);
                    else
                        values.Add(new List<object>(aObjects).ToArray());
                }
            }
        }

        public DelimitedFileTable(IDelimitedFileReader delimitedFileReader, int namesIndex = 0, int headerCount = 0)
        {
            Read(delimitedFileReader, namesIndex, headerCount);
        }

        public DelimitedFileTable(List<DelimitedFileRow> DelimitedFileRowList, int namesIndex = 0, int headerCount = 0)
        {
            Read(DelimitedFileRowList, namesIndex, headerCount);
        }

        public DelimitedFileTable(object[,] data, int namesIndex = 0, int headerCount = 0)
        {
            Read(data, namesIndex, headerCount);
        }

        public bool Read(object[,] data, int namesIndex = 0, int headerCount = 0)
        {
            if (data == null)
                return false;

            int aRowsStart = data.GetLowerBound(0);
            int aRowsEnd = data.GetUpperBound(0);
            int aRowsCount = aRowsEnd - aRowsStart + 1;
            if (aRowsCount <= 0)
                return true;

            int aColumnsStart = data.GetLowerBound(1);
            int aColumnsEnd = data.GetUpperBound(1);
            int aColumnsCount = aColumnsEnd - aColumnsStart + 1;
            if (aColumnsCount <= 0)
                return true;

            names = new string[aColumnsCount];
            for (int i = aColumnsStart; i < aColumnsCount + aColumnsStart; i++)
                names[i - aColumnsStart] = data[namesIndex, i] as string;

            if (aRowsCount == 1)
                return true;

            header = new List<object[]>();
            if (headerCount > 0)
            {
                int aMin = Math.Min(headerCount + aRowsStart + 1, aRowsEnd);
                for (int i = aRowsStart + 1; i < aMin + aRowsStart; i++)
                {
                    object[] aValues = new object[aColumnsCount];
                    for (int j = aColumnsStart; j < aColumnsCount + aColumnsStart; j++)
                        aValues[j - aColumnsStart] = data[i, j];
                    header.Add(aValues);
                }
            }

            int min = Math.Min(aRowsStart, namesIndex);

            values = new List<object[]>();
            for (int i = headerCount + min + 1; i < aRowsCount + min; i++)
            {
                object[] aValues = new object[aColumnsCount];
                for (int j = aColumnsStart; j < aColumnsCount + aColumnsStart; j++)
                    aValues[j - aColumnsStart] = data[i, j];
                values.Add(aValues);
            }

            return true;
        }

        public bool Read(List<DelimitedFileRow> delimitedFileRows, int namesIndex = 0, int headerCount = 0)
        {
            int count = delimitedFileRows.Count;

            if (count <= namesIndex)
                return true;

            if (delimitedFileRows[namesIndex] != null)
                names = delimitedFileRows[namesIndex].ToArray();

            header = new List<object[]>();
            int min = namesIndex + 1;
            for (int i = min; i < min + headerCount; i++)
                if (delimitedFileRows[i] != null)
                    header.Add(delimitedFileRows[i].ToArray());

            min = namesIndex + headerCount + 1;
            values = new List<object[]>();
            for (int i = min; i < count; i++)
                if (delimitedFileRows[i] != null)
                    values.Add(delimitedFileRows[i].ToArray());

            return true;
        }

        public bool Read(IDelimitedFileReader delimitedFileReader, int namesIndex = 0, int headerCount = 0)
        {
            if (delimitedFileReader == null)
                return false;

            List<DelimitedFileRow> aDelimitedFileRowList = delimitedFileReader.Read();
            if (aDelimitedFileRowList == null)
                return false;

            return Read(aDelimitedFileRowList, namesIndex, headerCount);
        }

        public bool Write(IDelimitedFileWriter delimitedFileWriter)
        {
            if (names == null || names.Length == 0 || delimitedFileWriter == null)
                return false;

            delimitedFileWriter.Write(new DelimitedFileRow(Extract<string>(names)));

            if (header != null && header.Count > 0)
                header.ForEach(x => delimitedFileWriter.Write(new DelimitedFileRow(Extract(x))));

            if (values != null && values.Count > 0)
                values.ForEach(x => delimitedFileWriter.Write(new DelimitedFileRow(Extract(x))));

            return true;
        }

        public object this[int row, int column]
        {
            get
            {
                if (row >= values.Count)
                    return null;

                if (column >= values[row].Length)
                    return null;

                return values[row][column] as string;
            }
        }

        public object this[int row, string columnName]
        {
            get
            {
                if (row >= values.Count)
                    return null;

                int aIndex = GetColumnIndex(columnName);
                if (aIndex == -1)
                    return null;

                if (aIndex >= values[row].Length)
                    return null;

                return this[row, aIndex];
            }
        }

        public object[] this[int row]
        {
            get
            {
                return values[row];
            }
        }

        public int GetColumnIndex(string columnName)
        {
            if (names == null || columnName == null)
                return -1;

            for (int i = 0; i < names.Length; i++)
                if (names[i] == columnName)
                    return i;

            return -1;
        }

        public string GetColumnName(int index)
        {
            TryGetColumnName(index, out string result);
            return result;
        }

        public bool TryGetColumnName(int index, out string name)
        {
            name = null;
            
            if (names == null)
                return false;

            if (index < 0 || index >= names.Length)
                return false;

            name = names[index];
            return true;
        }

        public List<int> GetColumnIndexes(string columnText, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (columnText == null)
                return null;

            List<int> result = new List<int>();
            for (int i = 0; i < names.Length; i++)
                if (Query.Compare(names[i], columnText, textComparisonType, caseSensitive))
                    result.Add(i);

            return result;
        }

        public List<int> GetRowIndexes(int columnIndex, string text, TextComparisonType textComparisonType, bool caseSensitive = true)
        {
            if (columnIndex >= ColumnCount || columnIndex < 0)
                return null;

            List<int> result = new List<int>();
            for(int i=0; i < RowCount; i++)
            {
                string value = ToString(i, columnIndex);
                if (Query.Compare(value, text, textComparisonType, caseSensitive))
                    result.Add(i);
            }
            return result;
        }

        public int GetColumnIndex(object value, int headerIndex)
        {
            if (header == null && header.Count >= headerIndex)
                return -1;

            object[] values = header[headerIndex];
            if (values == null || values.Length == 0)
                return -1;

            for (int i = 0; i < values.Length; i++)
            {
                if(values[i] == null)
                {
                    if (value == null)
                        return i;
                    else
                        continue;
                }

                if (values[i].Equals(value))
                    return i;
            }

            return -1;
        }

        public bool TryGetValue<T>(int row, int column, out T value)
        {
            value = default(T);

            if (row >= values.Count)
                return false;

            if (column >= values[row].Length)
                return false;

            value = (T)values[row][column];
            return true;
        }

        public bool TryConvert<T>(int row, int column, out T value)
        {
            value = default;

            object @object_value;
            if (!TryGetValue(row, column, out object_value))
                return false;

            return Query.TryConvert<T>(@object_value, out value);
        }

        public string ToString(int row, int column)
        {
            if (row >= values.Count)
                return null;

            if (column >= values[row].Length)
                return null;

            object aValue = values[row][column];
            if (aValue == null)
                return null;

            return aValue.ToString();
        }

        public int RowCount
        {
            get
            {
                if (values == null)
                    return -1;

                return values.Count;
            }
        }

        public int ColumnCount
        {
            get
            {
                if (names == null)
                    return -1;

                return names.Length;
            }
        }

        public List<Tuple<string, object>> GetTupleList(int row)
        {
            if (names == null)
                return null;

            if (row >= values.Count)
                return null;

            if (row < 0)
                return null;

            List<Tuple<string, object>> aTupleList = new List<Tuple<string, object>>();
            for (int j = 0; j < names.Length; j++)
                if (j < values[row].Length)
                    aTupleList.Add(new Tuple<string, object>(names[j], values[row][j]));
                else
                    aTupleList.Add(new Tuple<string, object>(names[j], null));

            return aTupleList;
        }

        public List<string> GetColumnNames()
        {
            if (names == null)
                return null;

            return names.ToList();
        }

        public IEnumerable<object> GetUnqueValues(string columnName)
        {
            int aIndex = GetColumnIndex(columnName);
            if (aIndex == -1)
                return null;

            HashSet<object> aResult = new HashSet<object>();
            foreach (object[] aValues_Row in values)
                aResult.Add(aValues_Row[aIndex]);

            return aResult;
        }

        public DelimitedFileTable Extract(params string[] columnNames)
        {
            if (names == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.names = columnNames;

            if (values == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.values = new List<object[]>();

            if (values.Count == 0 || columnNames.Length == 0)
                return aDelimitedFileTable;

            List<int> aIndexList = columnNames.ToList().ConvertAll(x => GetColumnIndex(x));

            foreach (object[] aValues_Row_Old in values)
            {
                object[] aValues_Row_New = new object[aIndexList.Count];
                for (int i = 0; i < aIndexList.Count; i++)
                {
                    int aIndex = aIndexList[i];
                    if (aIndex == -1 || aValues_Row_Old.Length <= aIndex)
                        aValues_Row_New[i] = null;
                    else
                        aValues_Row_New[i] = aValues_Row_Old[aIndex];
                }
                aDelimitedFileTable.values.Add(aValues_Row_New);
            }

            return aDelimitedFileTable;
        }

        public DelimitedFileTable Filter(IEnumerable<int> rowIndexes)
        {
            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.names = names;
            aDelimitedFileTable.header = header;
            aDelimitedFileTable.values = new List<object[]>();

            foreach (int aIndex in rowIndexes)
                aDelimitedFileTable.values.Add(values[aIndex]);

            return aDelimitedFileTable;
        }

        public DelimitedFileTable Filter(string columnName, TextComparisonType textComparisonType, string text, bool caseSensitive = true, bool tryConvert = true)
        {
            if (names == null)
                return null;

            DelimitedFileTable delimitedFileTable = new DelimitedFileTable();
            delimitedFileTable.names = names;

            if (values == null)
                return delimitedFileTable;

            delimitedFileTable.values = new List<object[]>();

            if (values.Count == 0)
                return delimitedFileTable;

            int index = GetColumnIndex(columnName);
            if (index == -1)
                return delimitedFileTable;

            foreach (object[] values_Row in values)
            {
                if (values_Row == null || values_Row.Length <= index)
                    continue;

                object value = values_Row[index];

                if (tryConvert)
                {
                    string @string = null;
                    if (Query.TryConvert(value, out @string))
                        value = @string;
                }

                if (!(value is string || value == null))
                    continue;

                if (!Query.Compare(value as string, text, textComparisonType, caseSensitive))
                    continue;

                delimitedFileTable.values.Add(values_Row);
            }

            return delimitedFileTable;
        }

        public DelimitedFileTable Filter(string columnName, NumberComparisonType numberComparisonType, double value, bool tryConvert = true)
        {
            if (names == null)
                return null;

            DelimitedFileTable delimitedFileTable = new DelimitedFileTable();
            delimitedFileTable.names = names;

            if (values == null)
                return delimitedFileTable;

            delimitedFileTable.values = new List<object[]>();

            if (values.Count == 0)
                return delimitedFileTable;

            int index = GetColumnIndex(columnName);
            if (index == -1)
                return delimitedFileTable;

            foreach (object[] values_Row in values)
            {
                if (values_Row == null || values_Row.Length <= index)
                    continue;

                object value_Temp = values_Row[index];

                if (tryConvert)
                {
                    double @double = double.NaN;
                    if (Query.TryConvert(value, out @double))
                        value = @double;
                }

                if (!(value_Temp is double))
                    continue;

                if (!Query.Compare((double)value_Temp, value, numberComparisonType))
                    continue;

                delimitedFileTable.values.Add(values_Row);
            }

            return delimitedFileTable;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                names = null;
                values = null;

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return ((IEnumerable<object[]>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<object[]>)values).GetEnumerator();
        }

        private static string[] Extract<T>(T[] values)
        {
            if (values == null)
                return null;

            string[] aResult = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                    aResult[i] = string.Empty;
                else
                    aResult[i] = values[i].ToString();
            }
            return aResult;
        }
    }
}