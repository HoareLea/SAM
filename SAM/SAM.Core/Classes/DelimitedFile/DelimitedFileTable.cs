using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace SAM.Core
{
    public class DelimitedFileTable : IDisposable, IEnumerable<object[]>
    {
        private bool pDisposed = false;

        private string[] pNames;
        private List<object[]> header;
        private List<object[]> values;

        private DelimitedFileTable()
        {

        }

        public DelimitedFileTable(DelimitedFileTable DelimitedFileTable)
        {
            if (DelimitedFileTable == null)
                return;
            if (DelimitedFileTable.pNames != null)
                pNames = new List<string>(DelimitedFileTable.pNames).ToArray();

            if (DelimitedFileTable.header != null)
            {
                header = new List<object[]>();
                foreach (object[] aObjects in DelimitedFileTable.header)
                {
                    if (aObjects == null)
                        header.Add(null);
                    else
                        header.Add(new List<object>(aObjects).ToArray());
                }
            }

            if (DelimitedFileTable.values != null)
            {
                values = new List<object[]>();
                foreach (object[] aObjects in DelimitedFileTable.values)
                {
                    if (aObjects == null)
                        values.Add(null);
                    else
                        values.Add(new List<object>(aObjects).ToArray());
                }
            }

        }

        public DelimitedFileTable(IDelimitedFileReader DelimitedFileReader, int HeaderCount = 0)
        {
            Read(DelimitedFileReader, HeaderCount);
        }

        public DelimitedFileTable(List<DelimitedFileRow> DelimitedFileRowList, int HeaderCount = 0)
        {
            Read(DelimitedFileRowList, HeaderCount);
        }

        public DelimitedFileTable(object[,] Data, int HeaderCount = 0)
        {
            Read(Data, HeaderCount);
        }

        public bool Read(object[,] Data, int HeaderCount = 0)
        {
            if (Data == null)
                return false;

            int aRowsStart = Data.GetLowerBound(0);
            int aRowsEnd = Data.GetUpperBound(0);
            int aRowsCount = aRowsEnd - aRowsStart + 1;
            if (aRowsCount <= 0)
                return true;

            int aColumnsStart = Data.GetLowerBound(1);
            int aColumnsEnd = Data.GetUpperBound(1);
            int aColumnsCount = aColumnsEnd - aColumnsStart + 1;
            if (aColumnsCount <= 0)
                return true;

            pNames = new string[aColumnsCount];
            for (int i = aColumnsStart; i < aColumnsCount + aColumnsStart; i++)
                pNames[i - aColumnsStart] = Data[aRowsStart, i] as string;

            if (aRowsCount == 1)
                return true;

            header = new List<object[]>();
            if (HeaderCount > 0)
            {
                int aMin = Math.Min(HeaderCount + aRowsStart + 1, aRowsEnd);
                for (int i = aRowsStart + 1; i < aMin + aRowsStart; i++)
                {
                    object[] aValues = new object[aColumnsCount];
                    for (int j = aColumnsStart; j < aColumnsCount + aColumnsStart; j++)
                        aValues[j - aColumnsStart] = Data[i, j];
                    header.Add(aValues);
                }
            }


            values = new List<object[]>();
            for (int i = HeaderCount + aRowsStart + 1; i < aRowsCount + aRowsStart; i++)
            {
                object[] aValues = new object[aColumnsCount];
                for (int j = aColumnsStart; j < aColumnsCount + aColumnsStart; j++)
                    aValues[j - aColumnsStart] = Data[i, j];
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
                pNames = delimitedFileRows[0].ToArray();

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

        public bool Read(IDelimitedFileReader DelimitedFileReader, int HeaderCount = 0)
        {
            if (DelimitedFileReader == null)
                return false;

            List<DelimitedFileRow> aDelimitedFileRowList = DelimitedFileReader.Read();
            if (aDelimitedFileRowList == null)
                return false;

            return Read(aDelimitedFileRowList, HeaderCount);
        }

        public bool Write(IDelimitedFileWriter DelimitedFileWriter)
        {
            if (pNames == null || pNames.Length == 0 || DelimitedFileWriter == null)
                return false;

            DelimitedFileWriter.Write(new DelimitedFileRow(Extract<string>(pNames)));

            if (header != null && header.Count > 0)
                header.ForEach(x => DelimitedFileWriter.Write(new DelimitedFileRow(Extract(x))));

            if (values != null && values.Count > 0)
                values.ForEach(x => DelimitedFileWriter.Write(new DelimitedFileRow(Extract(x))));

            return true;
        }

        public object this[int i, int j]
        {
            get
            {
                if (i >= values.Count)
                    return null;

                if (j >= values[i].Length)
                    return null;

                return values[i][j] as string;
            }
        }

        public object this[int i, string Name]
        {
            get
            {
                if (i >= values.Count)
                    return null;

                int aIndex = GetIndex(Name);
                if (aIndex == -1)
                    return null;

                if (aIndex >= values[i].Length)
                    return null;

                return this[i, aIndex];
            }
        }

        public object[] this[int i]
        {
            get
            {
                return values[i];
            }
        }

        public int GetIndex(string Name)
        {
            if (pNames == null || Name == null)
                return -1;

            for (int i = 0; i < pNames.Length; i++)
                if (pNames[i] == Name)
                    return i;

            return -1;
        }

        public bool TryGetValue<T>(int i, int j, out T Value)
        {
            Value = default(T);

            if (i >= values.Count)
                return false;

            if (j >= values[i].Length)
                return false;

            Value = (T)values[i][j];
            return true;
        }

        public string ToString(int i, int j)
        {
            if (i >= values.Count)
                return null;

            if (j >= values[i].Length)
                return null;

            object aValue = values[i][j];
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

        public List<Tuple<string, object>> GetTupleList(int i)
        {
            if (pNames == null)
                return null;

            if (i >= values.Count)
                return null;

            if (i < 0)
                return null;

            List<Tuple<string, object>> aTupleList = new List<Tuple<string, object>>();
            for (int j = 0; j < pNames.Length; j++)
                if (j < values[i].Length)
                    aTupleList.Add(new Tuple<string, object>(pNames[j], values[i][j]));
                else
                    aTupleList.Add(new Tuple<string, object>(pNames[j], null));

            return aTupleList;
        }

        public List<string> GetNameList()
        {
            if (pNames == null)
                return null;

            return pNames.ToList();
        }

        public IEnumerable<object> GetUnqueValues(string Name)
        {
            int aIndex = GetIndex(Name);
            if (aIndex == -1)
                return null;

            HashSet<object> aResult = new HashSet<object>();
            foreach (object[] aValues_Row in values)
                aResult.Add(aValues_Row[aIndex]);

            return aResult;
        }

        public DelimitedFileTable Extract(params string[] Names)
        {
            if (pNames == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.pNames = Names;

            if (values == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.values = new List<object[]>();

            if (values.Count == 0 || Names.Length == 0)
                return aDelimitedFileTable;

            List<int> aIndexList = Names.ToList().ConvertAll(x => GetIndex(x));

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

        public DelimitedFileTable Filter(string Name, object Value)
        {
            if (pNames == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.pNames = pNames;

            if (values == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.values = new List<object[]>();

            if (values.Count == 0)
                return aDelimitedFileTable;

            int aIndex = GetIndex(Name);
            if (aIndex == -1)
                return aDelimitedFileTable;

            foreach (object[] aValues_Row in values)
            {
                if (aValues_Row == null || aValues_Row.Length <= aIndex)
                    continue;

                object aValue = aValues_Row[aIndex];

                if (Value == null && aValue == null)
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }

                if (Value == null || aValue == null)
                    continue;

                if (Value.Equals(aValue))
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }
            }

            return aDelimitedFileTable;
        }

        public DelimitedFileTable Filter(string Name, object Value, bool TryToConvert)
        {
            if (pNames == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.pNames = pNames;

            if (values == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.values = new List<object[]>();

            if (values.Count == 0)
                return aDelimitedFileTable;

            int aIndex = GetIndex(Name);
            if (aIndex == -1)
                return aDelimitedFileTable;

            foreach (object[] aValues_Row in values)
            {
                if (aValues_Row == null || aValues_Row.Length <= aIndex)
                    continue;

                object aValue = aValues_Row[aIndex];

                if (Value == null && aValue == null)
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }

                if (Value == null || aValue == null)
                    continue;

                if (Value.Equals(aValue))
                {
                    aDelimitedFileTable.values.Add(aValues_Row);
                    continue;
                }

                if (!TryToConvert)
                    continue;

                Type aType_1 = Value.GetType();
                Type aType_2 = aValue.GetType();

                if (aType_1 == aType_2)
                    continue;

                if (aType_1 == typeof(string))
                {

                }
            }

            return aDelimitedFileTable;
        }

        public DelimitedFileTable Filter(IEnumerable<int> RowIndexes)
        {
            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.pNames = pNames;
            aDelimitedFileTable.header = header;
            aDelimitedFileTable.values = new List<object[]>();

            foreach (int aIndex in RowIndexes)
                aDelimitedFileTable.values.Add(values[aIndex]);

            return aDelimitedFileTable;
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (!pDisposed)
            {
                if (Disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                pNames = null;
                values = null;

                pDisposed = true;
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


        private static string[] Extract<T>(T[] Values)
        {
            if (Values == null)
                return null;

            string[] aResult = new string[Values.Length];
            for (int i = 0; i < Values.Length; i++)
            {
                if (Values[i] == null)
                    aResult[i] = string.Empty;
                else
                    aResult[i] = Values[i].ToString();
            }
            return aResult;
        }
    }
}
