// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Globalization;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string CamelCase(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return new CultureInfo("en-US", false).TextInfo.ToTitleCase(text.ToLower());
        }
    }
}
