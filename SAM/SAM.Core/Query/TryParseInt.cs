// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryParseInt(this string value, out int result)
        {
            result = default;

            if (TryParseDouble(value, out double @double))
            {
                result = System.Convert.ToInt32(@double);
                return true;
            }

            return false;
        }
    }
}
