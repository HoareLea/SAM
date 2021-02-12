namespace SAM.Core
{
    public static partial class Query
    {
        public static T[,] Transpose<T>(this T[,] values)
        {
            if (values == null)
                return null;

            int rowCount = values.GetLength(0);
            int columnCount = values.GetLength(1);

            T[,] result = new T[columnCount, rowCount];

            for (int i = 0; i < rowCount; i++)
                for (int j = 0; j < columnCount; j++)
                    result[j, i] = values[i, j];

            return result;
        }

        public static T[,] Transpose<T>(this T[] values)
        {
            if (values == null)
                return null;

            int rowCount = values.Length;

            T[,] result = new T[rowCount, 1];
            for (int i = 0; i < rowCount; i++)
                result[i, 0] = values[i];

            return result;
        }
    }
}