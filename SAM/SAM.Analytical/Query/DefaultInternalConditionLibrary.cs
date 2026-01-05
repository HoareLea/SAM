// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical InternalConditionLibrary
        /// </summary>
        /// <returns name="constructionLibrary"> Default SAM Analytical InternalConditionLibrary</returns>
        /// <search>Default SAM Analytical InternalCondition Library</search> 
        public static InternalConditionLibrary DefaultInternalConditionLibrary()
        {
            return ActiveSetting.Setting.GetValue<InternalConditionLibrary>(AnalyticalSettingParameter.DefaultInternalConditionLibrary);
        }
    }
}
