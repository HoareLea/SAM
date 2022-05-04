namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string ResourcesDirectory()
        {
            return Core.Query.ResourcesDirectory(Core.ActiveSetting.Setting, typeof(ActiveSetting).Assembly);
        }
    }
}