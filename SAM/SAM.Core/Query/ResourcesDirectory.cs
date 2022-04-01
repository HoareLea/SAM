using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string ResourcesDirectory(this Setting setting)
        {
            if (setting == null)
                return null;

            string resourcesDirectoryName = setting?.GetValue<string>(CoreSettingParameter.ResourcesDirectoryName);

            string result = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(UserSAMDirectory()), resourcesDirectoryName);
            if(!System.IO.Directory.Exists(result))
            {
                result = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExecutingAssemblyDirectory()), resourcesDirectoryName);
            }

            if (!System.IO.Directory.Exists(result))
            {
                result = System.IO.Path.Combine(ExecutingAssemblyDirectory(), resourcesDirectoryName);
            }

            return result;
        }

        public static string ResourcesDirectory()
        {
            return ResourcesDirectory(ActiveSetting.Setting);
        }

        public static string ResourcesDirectory(this Setting setting, Assembly assembly)
        {
            if (setting == null)
                return null;

            string resourcesDirectoryName = setting?.GetValue<string>(CoreSettingParameter.ResourcesDirectoryName);
            if (string.IsNullOrWhiteSpace(resourcesDirectoryName))
                return null;

            string name = assembly.GetName().Name;
            if (name.StartsWith("SAM."))
                name = name.Substring(4);

            name = name.Replace(".", @"\");

            string result = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(UserSAMDirectory()), resourcesDirectoryName, name);
            if(!System.IO.Directory.Exists(result))
            {
                result = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExecutingAssemblyDirectory()), resourcesDirectoryName, name);
            }

            if (!System.IO.Directory.Exists(result))
            {
                result = System.IO.Path.Combine(ExecutingAssemblyDirectory(), resourcesDirectoryName, name);
            }

            return result;
        }

        public static string ResourcesDirectory(Assembly assembly)
        {
            return ResourcesDirectory(ActiveSetting.Setting, assembly);
        }
    }
}