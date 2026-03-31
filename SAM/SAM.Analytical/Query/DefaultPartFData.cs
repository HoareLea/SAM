// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Reflection;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static PartFData DefaultPartFData()
        {
            return ActiveSetting.Setting.GetValue<PartFData>(AnalyticalSettingParameter.PartFData);
        }
    }
}
