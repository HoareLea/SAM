// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default TM59 SAM Analytical InternalConditionLibrary
        /// </summary>
        /// <returns name="constructionLibrary"> Default TM59 SAM Analytical InternalConditionLibrary</returns>
        /// <search>Default SAM Analytical InternalCondition Library</search>
        public static InternalConditionLibrary DefaultInternalConditionLibrary_TM59()
        {
            InternalConditionLibrary result = ActiveSetting.Setting?.GetValue<InternalConditionLibrary>(AnalyticalSettingParameter.DefaultInternalConditionLibrary_TM59);
            if (result != null)
                return result;

            // Falls back to reading the resource file directly, by its known literal name (matching
            // ActiveSetting.GetDefault()) - NOT via ActiveSetting.Setting.DefaultPath(DefaultInternalConditionLibraryFileName_TM59),
            // because a persisted %APPDATA%\SAM\settings Setting that predates the TM59 keys is missing
            // THAT parameter too (only GetDefault() sets it), so routing through it here would silently
            // fail the exact upgrade scenario this fallback exists to handle.
            string resourcesDirectory = Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(resourcesDirectory))
                return null;

            string path = System.IO.Path.Combine(resourcesDirectory, "SAM_InternalConditionLibrary_TM59.JSON");
            if (!System.IO.File.Exists(path))
                return null;

            return Core.Create.IJSAMObject<InternalConditionLibrary>(System.IO.File.ReadAllText(path));
        }
    }
}
