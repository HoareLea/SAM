namespace SAM.Core
{
    public static partial class Query
    {
        public static int ParseInt(this string value, int @default = default)
        {
            if(!TryParseInt(value, out int result))
            {
                result = @default;
            }

            return result;
        }
    }
}