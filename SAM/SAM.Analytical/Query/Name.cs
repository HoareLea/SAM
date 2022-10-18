namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string Name(string uniqueName, bool includePrefix = true, bool includeName = true, bool includeGuid = true, bool includeId = true)
        {
            if(string.IsNullOrWhiteSpace(uniqueName))
            {
                return uniqueName;
            }

            if(!UniqueNameDecomposition(uniqueName, out string prefix, out string name, out System.Guid? guid, out int id))
            {
                return uniqueName;
            }

            if(!includePrefix)
            {
                prefix = null;
            }

            if(!includeName)
            {
                name = null;
            }

            if(!includeGuid)
            {
                guid = null;
            }

            if(!includeId)
            {
                id = -1;
            }

            return UniqueName(prefix, name, guid, id);
        }
    }
}