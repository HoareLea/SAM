// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;

namespace SAM.Core
{
    public interface IParameterData
    {
        ParameterProperties ParameterProperties { get; }
        ParameterValue ParameterValue { get; }
    }
}
