namespace SAM.Core
{
    public static partial class Query
    {
        public static bool ValidFileName(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            return fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0;
        }
    }
}