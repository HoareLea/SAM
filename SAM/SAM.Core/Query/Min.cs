// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static T Min<T>(this IEnumerable<Range<T>> ranges)
        {
            if (ranges == null)
            {
                return default;
            }

            List<T> values = new List<T>();
            foreach (Range<T> range in ranges)
            {
                values.Add(range.Min);
            }

            if (values.Count == 0)
            {
                return default;
            }

            return values.Min();
        }
    }
}
