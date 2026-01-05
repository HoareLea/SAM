// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public interface IParameterizedSAMObject : IJSAMObject
    {
        object GetValue(Enum @enum);
        T GetValue<T>(Enum @enum);
        bool HasValue(Enum @enum);
        bool RemoveValue(Enum @enum);
        bool SetValue(Enum @enum, object value);
        bool TryGetValue<T>(Enum @enum, out T value, bool tryConvert = true);
        bool TryGetValue(Enum @enum, out object value);
    }
}
