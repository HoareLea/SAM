using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string ResourcesDirectoryName(this Setting setting)
        {
            if (setting == null)
                return null;

            string result;
            if (setting.TryGetValue(ActiveSetting.Name.DirectoryName_Resources, out result))
                return result;

            return null;
        }

        public static string ResourcesDirectoryName()
        {
            return ResourcesDirectoryName(ActiveSetting.Setting);
        }
    }
}