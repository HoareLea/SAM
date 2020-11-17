using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string DefaultInternalConditionLibraryPath()
        {
            return DefaultInternalConditionLibraryPath(ActiveSetting.Setting);
        }

        public static string DefaultInternalConditionLibraryPath(Setting setting)
        {
            if (setting == null)
                return null;
            
            string fileName;
            if (!setting.TryGetValue(AnalyticalSettingParameter.DefaultInternalConditionLibraryFileName, out fileName) || string.IsNullOrWhiteSpace(fileName))
                return null;

            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            string resourcesDirectory = Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(resourcesDirectory))
                return null;

            return System.IO.Path.Combine(resourcesDirectory, fileName);
        }
    }
}