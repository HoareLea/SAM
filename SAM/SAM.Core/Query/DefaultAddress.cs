namespace SAM.Core
{
    public static partial class Query
    {
        public static Address DefaultAddress()
        {
            return new Address(string.Empty, "London", string.Empty, Core.CountryCode.GB);
        }
    }
}