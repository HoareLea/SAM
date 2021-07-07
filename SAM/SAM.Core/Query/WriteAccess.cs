namespace SAM.Core
{
    public static partial class Query
    {
        public static bool WriteAccess(this AccessType accessType)
        {
            switch(accessType)
            {
                case AccessType.Write:
                case AccessType.ReadWrite:
                    return true;
                default:
                    return false;
            }
        }
    }
}