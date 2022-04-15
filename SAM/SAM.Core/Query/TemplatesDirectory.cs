using System;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string TemplatesDirectory(Assembly assembly)
        {
            string resourcesDirectory =  ResourcesDirectory(ActiveSetting.Setting, assembly);
            if(string.IsNullOrWhiteSpace(resourcesDirectory))
            {
                return null;
            }

            string templatesDirectoryName = ActiveSetting.Setting.GetValue<string>(CoreSettingParameter.TemplatesDirectoryName);
            if(string.IsNullOrWhiteSpace(templatesDirectoryName))
            {
                return null;
            }

            return System.IO.Path.Combine(resourcesDirectory, templatesDirectoryName);
        }

        public static string TemplatesDirectory<T>(bool includeTypeDirectory = true)
        {
            Type type = typeof(T);

            string result = TemplatesDirectory(type.Assembly);
            if (string.IsNullOrWhiteSpace(result))
            {
                return null;
            }

            if (!includeTypeDirectory)
            {
                return result;
            }


            result = System.IO.Path.Combine(result, typeof(T).Name);

            return result;
        }
    }
}