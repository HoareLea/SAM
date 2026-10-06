// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static int CountOrDefault<T>(this IEnumerable<T> source, int fallback = 0)
        {
            if (source is IReadOnlyCollection<T> readOnlyCol)
                return readOnlyCol.Count;
            if (source is ICollection<T> col)
                return col.Count;
            return fallback;
        }

        public static int Count(this Range<int> range)
        {
            if (range == null)
            {
                return -1;
            }

            return range.Max - range.Min + 1;

        }

        public static int Count(Period period_Destination, Period period_Source = Core.Period.Hourly)
        {
            switch (period_Destination)
            {
                case Core.Period.Monthly:
                    switch (period_Source)
                    {
                        case Core.Period.Hourly:
                            return 720;

                        case Core.Period.Daily:
                            return 30;

                        case Core.Period.Weekly:
                            return 4;
                    }
                    break;

                case Core.Period.Weekly:
                    switch (period_Source)
                    {
                        case Core.Period.Hourly:
                            return 168;

                        case Core.Period.Daily:
                            return 7;
                    }
                    break;

                case Core.Period.Daily:
                    switch (period_Source)
                    {
                        case Core.Period.Hourly:
                            return 24;
                    }
                    break;
            }

            return -1;
        }
    }
}
