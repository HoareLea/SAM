// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default TM59 SAM Analytical InternalCondition TextMap
        /// </summary>
        /// <returns name="TextMap">Default TM59 SAM Analytical InternalCondition TextMap</returns>
        /// <search>Default SAM Analytical InternalCondition, IC, InternalCondition, TextMap</search>
        public static TextMap DefaultInternalConditionTextMap_TM59()
        {
            TextMap result = ActiveSetting.Setting?.GetValue<TextMap>(AnalyticalSettingParameter.InternalConditionTextMap_TM59);
            if (result != null)
                return result;

            // Falls back to reading the resource file directly, by its known literal name (matching
            // ActiveSetting.GetDefault()) - NOT via ActiveSetting.Setting.DefaultPath(DefaultInternaConditionTextMaplFileName_TM59),
            // because a persisted %APPDATA%\SAM\settings Setting that predates the TM59 keys is missing
            // THAT parameter too (only GetDefault() sets it), so routing through it here would silently
            // fail the exact upgrade scenario this fallback exists to handle.
            string resourcesDirectory = Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(resourcesDirectory))
                return null;

            string path = System.IO.Path.Combine(resourcesDirectory, "SAM_InternalConditionTextMap_TM59.JSON");
            if (!System.IO.File.Exists(path))
                return null;

            return Core.Create.IJSAMObject<TextMap>(System.IO.File.ReadAllText(path));
        }
    }
}
