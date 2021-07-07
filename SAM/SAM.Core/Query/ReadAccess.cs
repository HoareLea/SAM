namespace SAM.Core
{
    public static partial class Query
    {
        public static bool ReadAccess(this AccessType accessType)
        {
            switch(accessType)
            {
                case AccessType.Read:
                case AccessType.ReadWrite:
                    return true;
                default:
                    return false;
            }
        }
    }
}