using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string DefaultMaterialLibraryPath()
        {
            return DefaultMaterialLibraryPath(ActiveSetting.Setting);
        }

        public static string DefaultMaterialLibraryPath(Setting setting)
        {
            if (setting == null)
                return null;
            
            string resourcesDirectory = Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(resourcesDirectory))
                return null;

            string fileName;
            if (!setting.TryGetValue(AnalyticalSettingParameter.DefaultMaterialLibrary, out fileName) || string.IsNullOrWhiteSpace(fileName))
                return null;

            return System.IO.Path.Combine(resourcesDirectory, fileName);
        }
    }
}