// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [AssociatedTypes(typeof(Setting)), Description("Core Setting Parameter")]
    public enum CoreSettingParameter
    {
        [ParameterProperties("Resources Directory Name", "Resources Directory Name"), ParameterValue(ParameterType.String)] ResourcesDirectoryName,
        [ParameterProperties("Templates Directory Name", "Templates Directory Name"), ParameterValue(ParameterType.String)] TemplatesDirectoryName,
        [ParameterProperties("Special Character Maps Directory Name", "Special Character Maps Directory Name"), ParameterValue(ParameterType.String)] SpecialCharacterMapsDirectoryName,
    }
}
