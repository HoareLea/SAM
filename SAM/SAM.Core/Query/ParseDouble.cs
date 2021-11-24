namespace SAM.Core
{
    public static partial class Query
    {
        public static double ParseDouble(this string value, double @default = default)
        {
            if(!TryParseDouble(value, out double result))
            {
                result = @default;
            }

            return result;
        }
    }
}