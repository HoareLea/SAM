using System.Reflection;

namespace SAM.Core
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            public const string DirectoryName_Resources = "DirectoryName_Resources";
        }

        private static Setting setting = Load();

        private static Setting Load()
        {
            Setting setting = ActiveManager.GetSetting(Assembly.GetExecutingAssembly());
            if (setting == null)
                setting = GetDefault();

            return setting;
        }

        public static Setting Setting
        {
            get
            {
                return setting;
            }
        }

        public static Setting GetDefault()
        {
            Setting result = new Setting(Assembly.GetExecutingAssembly());

            result.Add(Name.DirectoryName_Resources, "resources");

            return result;
        }
    }
}