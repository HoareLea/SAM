// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool IsNullable(this Type type)
        {
            if (type == null)
                return false;

            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}
