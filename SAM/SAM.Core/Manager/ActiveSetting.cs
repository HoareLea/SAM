// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Reflection;

namespace SAM.Core
{
    public static partial class ActiveSetting
    {
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
            result.SetValue(CoreSettingParameter.ResourcesDirectoryName, "resources");
            result.SetValue(CoreSettingParameter.TemplatesDirectoryName, "templates");
            result.SetValue(CoreSettingParameter.SpecialCharacterMapsDirectoryName, "SpecialCharacterMaps");

            return result;
        }
    }
}
