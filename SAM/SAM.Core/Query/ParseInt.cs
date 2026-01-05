// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static int ParseInt(this string value, int @default = default)
        {
            if (!TryParseInt(value, out int result))
            {
                result = @default;
            }

            return result;
        }
    }
}
