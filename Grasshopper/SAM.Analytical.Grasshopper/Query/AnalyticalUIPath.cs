namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static string AnalyticalUIPath()
        {
            return System.IO.Path.Combine(Core.Query.ExecutingAssemblyDirectory(), "SAM Analytical.exe");
        }
    }
}