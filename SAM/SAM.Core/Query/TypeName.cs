// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Runtime.InteropServices;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string TypeName(this object @object)
        {
            if (@object == null)
            {
                return null;
            }

            return Marshal.IsComObject(@object) ? COMObjectTypeName(@object) : @object.GetType().Name;
        }
    }
}
