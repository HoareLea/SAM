namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static bool Obsolete(this IGH_SAMComponent gH_SAMComponent)
        {
            if (gH_SAMComponent == null)
                return false;
            
            string componentVersion = gH_SAMComponent.ComponentVersion;
            string latestComponentVersion = gH_SAMComponent.LatestComponentVersion;

            if (string.IsNullOrEmpty(componentVersion) || string.IsNullOrEmpty(latestComponentVersion))
                return false;

            return componentVersion != latestComponentVersion;
        }
    }
}