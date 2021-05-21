namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Replace<T>(this T[] values, T value_Old, T value_New)
        {
            if(values == null)
            {
                return false;
            }

            bool result = false;
            for(int i = 0; i < values.Length; i++)
            {
                if ((values[i] != null && values[i].Equals(value_Old)) || (values[i] == null && value_Old == null))
                {
                    values[i] = value_New;
                    result = true;
                }
            }

            return result;
        }
    }
}