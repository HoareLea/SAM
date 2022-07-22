namespace SAM.Core
{
    public static partial class Query
    {
        public static bool WaitToUnlock(string path, int waitTime = 1000, int count = 10)
        {
            if(string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return false;
            }

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);

            bool result = false;

            int i = 0;
            while(i <= count)
            {
                if(!fileInfo.Locked())
                {
                    result = true;
                    break;
                }
                System.Threading.Thread.Sleep(waitTime);
            }

            return result;
        }
    }
}