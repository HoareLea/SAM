namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static bool HasComponentVersion(this IGH_SAMComponent gH_SAMComponent)
        {
            return !string.IsNullOrWhiteSpace(gH_SAMComponent?.ComponentVersion);
        }
    }
}