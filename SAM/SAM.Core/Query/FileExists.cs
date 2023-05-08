using System;
using System.IO;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool FileExists(this string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                File.Exists(filePath);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}