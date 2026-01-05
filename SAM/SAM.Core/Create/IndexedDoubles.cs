// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors


namespace SAM.Core
{
    public static partial class Create
    {
        public static IndexedDoubles IndexedDoubles(this IndexedDoubles indexedDoubles, int start, int end)
        {
            if (indexedDoubles == null)
            {
                return null;
            }

            IndexedDoubles result = new IndexedDoubles();
            for (int i = start; i < end; i++)
            {
                if (!indexedDoubles.ContainsIndex(i))
                {
                    continue;
                }

                result.Add(i, indexedDoubles[i]);
            }

            return result;
        }

        public static IndexedDoubles IndexedDoubles(this IndexedDoubles indexedDoubles, int start, int end, double defaultValue)
        {
            if (indexedDoubles == null)
            {
                return null;
            }

            IndexedDoubles result = new IndexedDoubles();
            for (int i = start; i < end; i++)
            {
                if (!indexedDoubles.ContainsIndex(i))
                {
                    result.Add(i, defaultValue);
                }

                result.Add(i, indexedDoubles[i]);
            }

            return result;
        }
    }
}
