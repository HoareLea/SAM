using System.IO;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string UserSAMTemporaryDirectory()
        {
            return Path.Combine(Path.GetTempPath(), "SAM");
        }
    }
}