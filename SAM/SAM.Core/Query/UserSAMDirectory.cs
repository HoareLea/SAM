namespace SAM.Core
{
    public static partial class Query
    {
        public static string UserSAMDirectory()
        {
            return System.IO.Path.Combine(UserDocumentsDirectory(), "SAM");
        }
    }
}