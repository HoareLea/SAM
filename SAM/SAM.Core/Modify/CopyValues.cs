using System;
using System.Collections.Generic;
using System.Data;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static List<Enum> CopyValues<T>(this SAMObject sAMObject, DataRow dataRow, IEnumerable<KeyValuePair<Enum, string>> names)
        {
            if (sAMObject == null || dataRow == null || names == null)
            {
                return null;
            }

            Dictionary<Enum, string> dictionary = new Dictionary<Enum, string>();
            foreach (KeyValuePair<Enum, string> name in names)
            {
                dictionary[name.Key] = name.Value;
            }

            return CopyValues(sAMObject, dataRow, dictionary);
        }
        
        public static List<Enum> CopyValues(this SAMObject sAMObject, DataRow dataRow, Dictionary<Enum, string> names)
        {
            if(sAMObject == null || dataRow == null || names == null)
            {
                return null;
            }

            DataColumnCollection dataColumnCollection = dataRow.Table?.Columns;
            if(dataColumnCollection == null || dataColumnCollection.Count == 0)
            {
                return null;
            }

            object[] values = dataRow.ItemArray;

            List<Enum> result = new List<Enum>();
            foreach(KeyValuePair<Enum, string> keyValuePair in names)
            {
                if(string.IsNullOrEmpty(keyValuePair.Value))
                {
                    continue;
                }
                
                int index = dataColumnCollection.IndexOf(keyValuePair.Value);
                if(index == -1)
                {
                    continue;
                }

                if(!sAMObject.SetValue(keyValuePair.Key, values[index]))
                {
                    continue;
                }

                result.Add(keyValuePair.Key);
            }

            return result;
        }
    }
}