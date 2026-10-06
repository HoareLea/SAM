// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns the shared space use TextMap: the vocabulary that maps a room name to a
        /// <see cref="SpaceUse"/>, used by Approved Document F, Approved Document O, CIBSE TM59 and the
        /// SAM_UI internal condition mapping.
        /// </summary>
        /// <returns name="TextMap">Shared space use TextMap, or null where the resource is missing.</returns>
        /// <search>Default SAM Analytical SpaceUse, Space Use, TextMap, semantic classification</search>
        public static TextMap DefaultSpaceUseTextMap()
        {
            TextMap result = ActiveSetting.Setting?.GetValue<TextMap>(AnalyticalSettingParameter.SpaceUseTextMap);
            if (result != null)
            {
                return result;
            }

            //Falls back to reading the resource file directly, by its known literal name, for the same
            //reason DefaultInternalConditionTextMap_TM59 does: a persisted %APPDATA%\SAM\settings
            //Setting written before this key existed is missing the parameter entirely (only
            //ActiveSetting.GetDefault() sets it), so routing through Setting.DefaultPath here would
            //silently fail the exact upgrade case this fallback exists for. See SAM issue #64.
            string resourcesDirectory = Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(resourcesDirectory))
            {
                return null;
            }

            string path = System.IO.Path.Combine(resourcesDirectory, "SAM_SpaceUseTextMap.JSON");
            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            return Core.Create.IJSAMObject<TextMap>(System.IO.File.ReadAllText(path));
        }

        /// <summary>
        /// Returns a <see cref="SpaceSemanticsResolver"/> built on the shared space use TextMap.
        /// </summary>
        public static SpaceSemanticsResolver DefaultSpaceSemanticsResolver()
        {
            TextMap textMap = DefaultSpaceUseTextMap();
            if (textMap is null)
            {
                return null;
            }

            return new SpaceSemanticsResolver(textMap);
        }
    }
}
