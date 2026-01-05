// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SAM.Core
{
    public static partial class Query
    {
        public static MemoryStream MemoryStream(this string text)
        {
            if (text == null)
                return null;

            byte[] bytes = Encoding.ASCII.GetBytes(text);
            return new MemoryStream(bytes);
        }

        public static MemoryStream MemoryStream(this IEnumerable<string> text)
        {
            if (text == null)
                return null;

            byte[] bytes = Encoding.ASCII.GetBytes(string.Join("\n", text));
            return new MemoryStream(bytes);
        }
    }
}
