using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string DefaultMaterialLibraryPath()
        {
            string resourcesDirectory = Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(resourcesDirectory))
                return null;

            string fileName;
            if (ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.FileName_DefaultMaterialLibrary, out fileName))
                return fileName;

            return System.IO.Path.Combine(resourcesDirectory, fileName);
        }
    }
}