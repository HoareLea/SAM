// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    /// <summary>
    /// Class to store one delimited file row
    /// </summary>
    public class DelimitedFileRow : List<string>
    {
        public string LineText { get; set; }

        public DelimitedFileRow()
        {
        }

        public DelimitedFileRow(IEnumerable<string> values)
        {
            AddRange(values);
        }

        public bool TryGetValue<T>(int index, out T value)
        {
            value = default(T);
            if (index < 0 || index >= Count)
            {
                return false;
            }

            return Query.TryConvert(this[index], out value);
        }
    }
}
