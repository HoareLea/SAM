namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static string AnalyticalUIPath()
        {
            string fileName = "SAM Analytical.exe";

            string path = System.IO.Path.Combine(Core.Query.ExecutingAssemblyDirectory(), fileName);
            if(!System.IO.Path.Exists(path))
            {
                string path_Temp = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Core.Query.ExecutingAssemblyDirectory()), fileName);
                if (System.IO.Path.Exists(path))
                {
                    path = path_Temp;
                }
            }

            return path;
        }
    }
}