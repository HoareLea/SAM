// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Core
{
    public static partial class Query
    {
        public static T[,] Array<T>(this JsonArray jsonArray, T @default = default)
        {
            if (jsonArray == null)
            {
                return null;
            }

            List<T[]> values_Temp = new List<T[]>();
            int maxCount = 0;
            for (int i = 0; i < jsonArray.Count; i++)
            {
                JsonArray jsonArray_Temp = jsonArray[i] as JsonArray;
                if (jsonArray_Temp == null)
                {
                    continue;
                }

                T[] values_Temp_Temp = new T[jsonArray_Temp.Count];
                if (jsonArray_Temp.Count > maxCount)
                {
                    maxCount = jsonArray_Temp.Count;
                }

                for (int j = 0; j < jsonArray_Temp.Count; j++)
                {
                    //The JsonNode is handed to TryConvert whole rather than unwrapped with
                    //GetValue<object>() first. Unwrapping a node that came out of JsonNode.Parse yields a
                    //boxed JsonElement, which no CLR type test recognises, so every element of every saved
                    //file converted to default - a matrix of zeros rather than the stored numbers.
                    JsonNode jsonNode = jsonArray_Temp[j];
                    if (jsonNode == null)
                    {
                        values_Temp_Temp[j] = default;
                        continue;
                    }

                    if (!TryConvert(jsonNode, out T t))
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
