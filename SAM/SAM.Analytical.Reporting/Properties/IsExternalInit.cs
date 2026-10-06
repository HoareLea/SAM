// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Enables C# 9 <c>init</c> accessors on netstandard2.0, which does not ship this marker type.
    /// </summary>
    internal static class IsExternalInit
    {
    }
}
