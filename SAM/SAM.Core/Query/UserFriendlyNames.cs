// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<string> UserFriendlyNames(this object @object)
        {
            if (@object == null)
                return null;

            List<string> names = @object.Names(true, true, true);
            if (names == null || names.Count == 0)
                return names;

            HashSet<string> result = new HashSet<string>();
            foreach (string name in names)
            {
                result.Add(UserFriendlyName(name));
            }
            return result.ToList();
        }
    }
}
