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
        private List<object[]> pHeader;
        private List<object[]> pValues;

        private DelimitedFileTable()
        {

        }

        public DelimitedFileTable(DelimitedFileTable DelimitedFileTable)
        {
            if (DelimitedFileTable == null)
                return;
            if (DelimitedFileTable.pNames != null)
                pNames = new List<string>(DelimitedFileTable.pNames).ToArray();

            if (DelimitedFileTable.pHeader != null)
            {
                pHeader = new List<object[]>();
                foreach (object[] aObjects in DelimitedFileTable.pHeader)
                {
                    if (aObjects == null)
                        pHeader.Add(null);
                    else
                        pHeader.Add(new List<object>(aObjects).ToArray());
                }
            }

            if (DelimitedFileTable.pValues != null)
            {
                pValues = new List<object[]>();
                foreach (object[] aObjects in DelimitedFileTable.pValues)
                {
                    if (aObjects == null)
                        pValues.Add(null);
                    else
                        pValues.Add(new List<object>(aObjects).ToArray());
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

            pHeader = new List<object[]>();
            if (HeaderCount > 0)
            {
                int aMin = Math.Min(HeaderCount + aRowsStart + 1, aRowsEnd);
                for (int i = aRowsStart + 1; i < aMin + aRowsStart; i++)
                {
                    object[] aValues = new object[aColumnsCount];
                    for (int j = aColumnsStart; j < aColumnsCount + aColumnsStart; j++)
                        aValues[j - aColumnsStart] = Data[i, j];
                    pHeader.Add(aValues);
                }
            }


            pValues = new List<object[]>();
            for (int i = HeaderCount + aRowsStart + 1; i < aRowsCount + aRowsStart; i++)
            {
                object[] aValues = new object[aColumnsCount];
                for (int j = aColumnsStart; j < aColumnsCount + aColumnsStart; j++)
                    aValues[j - aColumnsStart] = Data[i, j];
                pValues.Add(aValues);
            }

            return true;
        }

        public bool Read(List<DelimitedFileRow> DelimitedFileRowList, int HeaderCount = 0)
        {
            int aCount = DelimitedFileRowList.Count;

            if (aCount == 0)
                return true;

            if (DelimitedFileRowList[0] != null)
                pNames = DelimitedFileRowList[0].ToArray();

            if (aCount == 1)
                return true;

            pHeader = new List<object[]>();
            int aMin = Math.Min(HeaderCount + 1, aCount);
            for (int i = 1; i < aMin; i++)
                if (DelimitedFileRowList[i] != null)
                    pHeader.Add(DelimitedFileRowList[i].ToArray());

            pValues = new List<object[]>();
            for (int i = aMin; i < aCount; i++)
                if (DelimitedFileRowList[i] != null)
                    pValues.Add(DelimitedFileRowList[i].ToArray());

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

            if (pHeader != null && pHeader.Count > 0)
                pHeader.ForEach(x => DelimitedFileWriter.Write(new DelimitedFileRow(Extract(x))));

            if (pValues != null && pValues.Count > 0)
                pValues.ForEach(x => DelimitedFileWriter.Write(new DelimitedFileRow(Extract(x))));

            return true;
        }

        public object this[int i, int j]
        {
            get
            {
                if (i >= pValues.Count)
                    return null;

                if (j >= pValues[i].Length)
                    return null;

                return pValues[i][j] as string;
            }
        }

        public object this[int i, string Name]
        {
            get
            {
                if (i >= pValues.Count)
                    return null;

                int aIndex = GetIndex(Name);
                if (aIndex == -1)
                    return null;

                if (aIndex >= pValues[i].Length)
                    return null;

                return this[i, aIndex];
            }
        }

        public object[] this[int i]
        {
            get
            {
                return pValues[i];
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

            if (i >= pValues.Count)
                return false;

            if (j >= pValues[i].Length)
                return false;

            Value = (T)pValues[i][j];
            return true;
        }

        public string ToString(int i, int j)
        {
            if (i >= pValues.Count)
                return null;

            if (j >= pValues[i].Length)
                return null;

            object aValue = pValues[i][j];
            if (aValue == null)
                return null;

            return aValue.ToString();
        }

        public int Count
        {
            get
            {
                if (pValues == null)
                    return -1;

                return pValues.Count;
            }
        }

        public List<Tuple<string, object>> GetTupleList(int i)
        {
            if (pNames == null)
                return null;

            if (i >= pValues.Count)
                return null;

            if (i < 0)
                return null;

            List<Tuple<string, object>> aTupleList = new List<Tuple<string, object>>();
            for (int j = 0; j < pNames.Length; j++)
                if (j < pValues[i].Length)
                    aTupleList.Add(new Tuple<string, object>(pNames[j], pValues[i][j]));
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
            foreach (object[] aValues_Row in pValues)
                aResult.Add(aValues_Row[aIndex]);

            return aResult;
        }

        public DelimitedFileTable Extract(params string[] Names)
        {
            if (pNames == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.pNames = Names;

            if (pValues == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.pValues = new List<object[]>();

            if (pValues.Count == 0 || Names.Length == 0)
                return aDelimitedFileTable;

            List<int> aIndexList = Names.ToList().ConvertAll(x => GetIndex(x));

            foreach (object[] aValues_Row_Old in pValues)
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
                aDelimitedFileTable.pValues.Add(aValues_Row_New);
            }

            return aDelimitedFileTable;
        }

        public DelimitedFileTable Filter(string Name, object Value)
        {
            if (pNames == null)
                return null;

            DelimitedFileTable aDelimitedFileTable = new DelimitedFileTable();
            aDelimitedFileTable.pNames = pNames;

            if (pValues == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.pValues = new List<object[]>();

            if (pValues.Count == 0)
                return aDelimitedFileTable;

            int aIndex = GetIndex(Name);
            if (aIndex == -1)
                return aDelimitedFileTable;

            foreach (object[] aValues_Row in pValues)
            {
                if (aValues_Row == null || aValues_Row.Length <= aIndex)
                    continue;

                object aValue = aValues_Row[aIndex];

                if (Value == null && aValue == null)
                {
                    aDelimitedFileTable.pValues.Add(aValues_Row);
                    continue;
                }

                if (Value == null || aValue == null)
                    continue;

                if (Value.Equals(aValue))
                {
                    aDelimitedFileTable.pValues.Add(aValues_Row);
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

            if (pValues == null)
                return aDelimitedFileTable;

            aDelimitedFileTable.pValues = new List<object[]>();

            if (pValues.Count == 0)
                return aDelimitedFileTable;

            int aIndex = GetIndex(Name);
            if (aIndex == -1)
                return aDelimitedFileTable;

            foreach (object[] aValues_Row in pValues)
            {
                if (aValues_Row == null || aValues_Row.Length <= aIndex)
                    continue;

                object aValue = aValues_Row[aIndex];

                if (Value == null && aValue == null)
                {
                    aDelimitedFileTable.pValues.Add(aValues_Row);
                    continue;
                }

                if (Value == null || aValue == null)
                    continue;

                if (Value.Equals(aValue))
                {
                    aDelimitedFileTable.pValues.Add(aValues_Row);
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
            aDelimitedFileTable.pHeader = pHeader;
            aDelimitedFileTable.pValues = new List<object[]>();

            foreach (int aIndex in RowIndexes)
                aDelimitedFileTable.pValues.Add(pValues[aIndex]);

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
                pValues = null;

                pDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            return ((IEnumerable<object[]>)pValues).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<object[]>)pValues).GetEnumerator();
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
