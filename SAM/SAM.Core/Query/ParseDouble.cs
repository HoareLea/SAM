// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static double ParseDouble(this string value, double @default = default)
        {
            if (!TryParseDouble(value, out double result))
            {
                result = @default;
            }

            return result;
        }
    }
}
