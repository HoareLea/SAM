// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    public interface IComplexModifier : IModifier
    {

    }

    public interface IComplexModifier<T> : IComplexModifier where T : IModifier
    {
        List<T> Modifiers { get; }
    }
}
