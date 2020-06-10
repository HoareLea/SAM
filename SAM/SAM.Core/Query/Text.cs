namespace SAM.Core
{
    public static partial class Query
    {
        public static string Text(this CountryCode countryCode)
        {
            return Description(countryCode);
        }
    }
}