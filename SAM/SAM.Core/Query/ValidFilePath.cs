using System;
using System.IO;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool ValidFilePath(this string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                File.Exists(filePath);
            }
            catch (Exception exception)
            {
                return false;
            }
            return true;
        }
    }
}