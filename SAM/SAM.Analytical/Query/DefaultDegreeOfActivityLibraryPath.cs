using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string DefaultDegreeOfActivityLibraryPath()
        {
            return DefaultDegreeOfActivityLibraryPath(ActiveSetting.Setting);
        }

        public static string DefaultDegreeOfActivityLibraryPath(Setting setting)
        {
            if (setting == null)
                return null;
            
            string fileName;
            if (!setting.TryGetValue(AnalyticalSettingParameter.DefaultDegreeOfActivityLibraryFileName, out fileName) || string.IsNullOrWhiteSpace(fileName))
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