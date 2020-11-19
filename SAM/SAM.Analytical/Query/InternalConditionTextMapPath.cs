using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string InternalConditionTextMapPath()
        {
            return InternalConditionTextMapPath(ActiveSetting.Setting);
        }

        public static string InternalConditionTextMapPath(Setting setting)
        {
            if (setting == null)
                return null;
            
            string fileName;
            if (!setting.TryGetValue(AnalyticalSettingParameter.InternaConditionTextMaplFileName, out fileName) || string.IsNullOrWhiteSpace(fileName))
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