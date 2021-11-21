using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Create
    {
        public static T[,] Array<T>(string path, string separator = "\t", bool removeEmptyLines = true, T @default = default)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return null;
            }

            string[] lines = System.IO.File.ReadAllLines(path);
            if (lines == null || lines.Length == 0)
            {
                return null;
            }

            int maxCount = -1;

            List<List<T>> values = new List<List<T>>();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) && removeEmptyLines)
                {
                    continue;
                }

                string[] valueStrings = line.Split(new string[] { separator }, System.StringSplitOptions.None);
                if (valueStrings.Length > maxCount)
                {
                    maxCount = valueStrings.Length;
                }

                List<T> row = new List<T>();
                foreach (string valueString in valueStrings)
                {
                    if (!Query.TryConvert(valueString, out T value))
                    {
                        value = @default;
                    }

                    row.Add(value);
                }

                values.Add(row);
            }

            T[,] result = new T[values.Count, maxCount];
            for (int i = 0; i < values.Count; i++)
            {
                for (int j = 0; j < values[i].Count; j++)
                {
                    result[i, j] = values[i][j];
                }

                for(int j = values[i].Count; j < maxCount; j++)
                {
                    result[i, j] = @default;
                }
            }

            return result;
        }
    }
}