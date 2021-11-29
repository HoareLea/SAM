using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<int> FindIndexes<T>(this DataTable dataTable, string columnName, IEnumerable<T> values)
        {
            if(dataTable == null || columnName == null)
            {
                return null;
            }

            DataColumnCollection dataColumnCollection = dataTable.Columns;
            if(dataColumnCollection == null || dataColumnCollection.Count == 0)
            {
                return null;
            }

            int index = dataTable.Columns.IndexOf(columnName);
            if(index == -1)
            {
                return null;
            }

            DataRowCollection dataRowCollection = dataTable.Rows;
            if(dataRowCollection == null || dataRowCollection.Count == 0)
            {
                return null;
            }

            int count = values.Count();

            List<int> result = Enumerable.Repeat(-1, count).ToList();
            for(int i =0; i < dataColumnCollection.Count; i++)
            {
                if (!TryConvert(dataRowCollection[i][index], out T value))
                {
                    continue;
                }

                for(int j = 0; j < count; j++)
                {
                    T value_Temp = values.ElementAt(j);

                    if (value == null)
                    {
                        if (value_Temp == null)
                        {
                            result[j] = i;
                        }
                    }
                    else
                    {
                        if(value.Equals(value_Temp))
                        {
                            result[j] = i;
                        }
                    }
                }
            }

            return result;
        }

        public static List<int> FindIndexes<T>(this DataTable dataTable, string columnName, T value)
        {
            if (dataTable == null || columnName == null)
            {
                return null;
            }

            DataColumnCollection dataColumnCollection = dataTable.Columns;
            if (dataColumnCollection == null || dataColumnCollection.Count == 0)
            {
                return null;
            }

            int index = dataTable.Columns.IndexOf(columnName);
            if (index == -1)
            {
                return null;
            }

            DataRowCollection dataRowCollection = dataTable.Rows;
            if (dataRowCollection == null || dataRowCollection.Count == 0)
            {
                return null;
            }

            List<int> result = new List<int>();
            for (int i = 0; i < dataRowCollection.Count; i++)
            {
                if (!TryConvert(dataRowCollection[i][index], out T value_Temp))
                {
                    continue;
                }

                if (value == null)
                {
                    if (value_Temp == null)
                    {
                        result.Add(i);
                    }
                }
                else
                {
                    if (value.Equals(value_Temp))
                    {
                        result.Add(i);
                    }
                }
            }

            return result;
        }
    }
}