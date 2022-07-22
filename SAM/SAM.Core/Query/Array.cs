using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static T[,] Array<T>(this JArray jArray, T @default = default)
        {
            if (jArray == null)
            {
                return null;
            }

            List<T[]> values_Temp = new List<T[]>();
            int maxCount = 0;
            for (int i = 0; i < jArray.Count; i++)
            {
                JArray jArray_Temp = jArray[i].Value<JArray>();
                if (jArray_Temp == null)
                {
                    continue;
                }

                T[] values_Temp_Temp = new T[jArray_Temp.Count];
                if (jArray_Temp.Count > maxCount)
                {
                    maxCount = jArray_Temp.Count;
                }

                for (int j = 0; j < jArray_Temp.Count; j++)
                {
                    object @object = jArray_Temp[j].Value<object>();
                    if(@object == null)
                    {
                        values_Temp_Temp[j] = default;
                        continue;
                    }
                    
                    if(!TryConvert(@object, out T t))
                    {
                        values_Temp_Temp[j] = default;
                        continue;
                    }

                    values_Temp_Temp[j] = t;
                }

                values_Temp.Add(values_Temp_Temp);
            }

            T[,] result = new T[values_Temp.Count, maxCount];
            for (int i = 0; i < values_Temp.Count; i++)
            {
                for (int j = 0; j < values_Temp[i].Length; j++)
                {
                    result[i, j] = values_Temp[i][j];
                }
            }

            return result;

        }
    }
}