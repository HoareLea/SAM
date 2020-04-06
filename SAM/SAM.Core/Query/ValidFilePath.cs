using System;


namespace SAM.Core
{
    public static partial class Query
    {
        public static bool ValidFilePath(this string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            return Uri.IsWellFormedUriString(filePath, UriKind.RelativeOrAbsolute);
        }
    }
}
