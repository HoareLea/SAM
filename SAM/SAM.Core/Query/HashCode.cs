namespace SAM.Core
{
    public static partial class Query
    {
        public static int HashCode(params int[] values)
        {
            if (values == null)
            {
                return 0;
            }

            int result = 17;
            foreach (int value in values)
            {
                result = result * 31 + value;
            }

            return result;
        }
    }
}