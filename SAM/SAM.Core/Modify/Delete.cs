using System;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool Delete(string path, int maxDays_CreationTime)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return false;

            DateTime dateTime_Write = System.IO.File.GetCreationTime(path);
            DateTime dateTime_Now = DateTime.Now;

            int days = (dateTime_Now - dateTime_Write).Days;
            if (days < maxDays_CreationTime)
                return false;
            try
            {
                System.IO.File.Delete(path);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}