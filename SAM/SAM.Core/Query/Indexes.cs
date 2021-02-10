using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<int> Indexes(this DelimitedFileTable delimitedFileTable, IEnumerable<SAMObject> sAMObjects, int columnIndex, string parameterName)
        {
            if (delimitedFileTable == null || sAMObjects == null || columnIndex  == -1 || parameterName == null)
                return null;

            object[] columnValues = delimitedFileTable.GetColumnValues(columnIndex);
            if (columnValues == null)
                return null;

            List<int> result = new List<int>();
            foreach(SAMObject sAMObject in sAMObjects)
            {
                if(!sAMObject.TryGetValue(parameterName, out object value_1))
                {
                    result.Add(-1);
                    continue;
                }

                System.Type type = value_1?.GetType();

                for (int i =0; i < columnValues.Length; i++)
                {
                    object value_Temp = columnValues[i];

                    if(value_1 == null && value_Temp == null)
                    {
                        result.Add(i);
                        break;
                    }

                    if (!TryConvert(value_Temp, out object value_2, type))
                        continue;

                    if (!value_1.Equals(value_2))
                        continue;

                    result.Add(i);
                    break;
                }
            }

            return result;
        }
    }
}