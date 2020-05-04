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

        public DelimitedFileTable(IDelimitedFileReader delimitedFileReader, int headerCount = 0)
        {
            Read(delimitedFileReader, headerCount);
        }

        public DelimitedFileTable(List<DelimitedFileRow> DelimitedFileRowList, int HeaderCount = 0)
        {
            Read(DelimitedFileRowList, HeaderCount);
        }

        public DelimitedFileTable(object[,] data, int headerCount = 0)
        {
            Read(data, headerCount);
        }

        public bool Read(object[,] data, int headerCount = 0)
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
                names[i - aColumnsStart] = data[aRowsStart, i] as string;

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

            values = new List<object[]>();
            for (int i = headerCount + aRowsStart + 1; i < aRowsCount + aRowsStart; i++)
            {
                object[] aValues = new object[aColumnsCount];
                for (int j = aColumnsStart; j < aColumnsCount + aColumnsStart; j++)
                    aValues[j - aColumnsStart] = data[i, j];
                values.Add(aValues);
            }

            return true;
        }

        public bool Read(List<DelimitedFileRow> delimitedFileRows, int headerCount = 0)
        {
            int count = delimitedFileRows.Count;

            if (count == 0)
                return true;

            if (delimitedFileRows[0] != null)
                names = delimitedFileRows[0].ToArray();

            if (count == 1)
                return true;

            header = new List<object[]>();
            int min = Math.Min(headerCount + 1, count);
            for (int i = 1; i < min; i++)
                if (delimitedFileRows[i] != null)
                    header.Add(delimitedFileRows[i].ToArray());

            values = new List<object[]>();
            for (int i = min; i < count; i++)
                if (delimitedFileRows[i] != null)
                    values.Add(delimitedFileRows[i].ToArray());

            return true;
        }

        public bool Read(IDelimitedFileReader delimitedFileReader, int headerCount = 0)
        {
            if (delimitedFileReader == null)
                return false;

            List<DelimitedFileRow> aDelimitedFileRowList = delimitedFileReader.Read();
            if (aDelimitedFileRowList == null)
                return false;

            return Read(aDelimitedFileRowList, headerCount);
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

                int aIndex = GetIndex(columnName);
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

        public int GetIndex(string columnName)
        {
            if (names == null || columnName == null)
                return -1;

            for (int i = 0; i < names.Length; i++)
                if (names[i] == columnName)
                    return i;

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

        public int Count
        {
            get
            {
                if (values == null)
                    return -1;

                return values.Count;
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
            int aIndex = GetIndex(columnName);
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

            List<int> aIndexList = columnNames.ToList().ConvertAll(x => GetIndex(x));

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

        public DelimitedFileTable Filter(string columnName, object value)
        {
            if (names == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.names = names;

            if (values == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.values = new List<object[]>();

            if (values.Count == 0)
                return aDelimitedFileTable;

            int aIndex = GetIndex(columnName);
            if (aIndex == -1)
                return aDelimitedFileTable;

            foreach (object[] aValues_Row in values)
            {
                if (aValues_Row == null || aValues_Row.Length <= aIndex)
                    continue;

                object aValue = aValues_Row[aIndex];

                if (value == null && aValue == null)
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }

                if (value == null || aValue == null)
                    continue;

                if (value.Equals(aValue))
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }
            }

            return aDelimitedFileTable;
        }

        public DelimitedFileTable Filter(string columnName, object value, bool tryToConvert)
        {
            if (names == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.names = names;

            if (values == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.values = new List<object[]>();

            if (values.Count == 0)
                return aDelimitedFileTable;

            int aIndex = GetIndex(columnName);
            if (aIndex == -1)
                return aDelimitedFileTable;

            foreach (object[] aValues_Row in values)
            {
                if (aValues_Row == null || aValues_Row.Length <= aIndex)
                    continue;

                object aValue = aValues_Row[aIndex];

                if (value == null && aValue == null)
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }

                if (value == null || aValue == null)
                    continue;

                if (value.Equals(aValue))
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }

                if (!tryToConvert)
                    continue;

                Type aType_1 = value.GetType();
                Type aType_2 = aValue.GetType();

                if (aType_1 == aType_2)
                    continue;

                if (aType_1 == typeof(string))
                {
                }
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