using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string ResourcesDirectory(this Setting setting)
        {
            if (setting == null)
                return null;

            return System.IO.Path.Combine(ExecutingAssemblyDirectory(), ResourcesDirectoryName(setting));
        }

        public static string ResourcesDirectory()
        {
            return ResourcesDirectory(ActiveSetting.Setting);
        }

        public static string ResourcesDirectory(this Setting setting, Assembly assembly)
        {
            if (setting == null)
                return null;

            string resourcesDirectoryName = ResourcesDirectoryName(setting);
            if (string.IsNullOrWhiteSpace(resourcesDirectoryName))
                return null;

            return System.IO.Path.Combine(ExecutingAssemblyDirectory(), resourcesDirectoryName, assembly.GetName().Name);
        }

        public static string ResourcesDirectory(Assembly assembly)
        {
            return ResourcesDirectory(ActiveSetting.Setting, assembly);
        }
    }
}