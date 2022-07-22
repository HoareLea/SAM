using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string UserDocumentsDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}