using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string DefaultConstructionLibraryPath()
        {
            return DefaultConstructionLibraryPath(ActiveSetting.Setting);
        }

        public static string DefaultConstructionLibraryPath(Setting setting)
        {
            if (setting == null)
                return null;
            
            string fileName;
            if (!setting.TryGetValue(ActiveSetting.Name.FileName_DefaultConstructionLibrary, out fileName) || string.IsNullOrWhiteSpace(fileName))
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