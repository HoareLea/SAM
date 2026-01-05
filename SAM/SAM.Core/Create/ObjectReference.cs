// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public static partial class Create
    {
        public static ObjectReference ObjectReference<T>(this T @object, Func<T, Reference> func = null)
        {
            if (@object == null)
            {
                return null;
            }

            return new ObjectReference(@object.GetType(), func?.Invoke(@object));
        }
    }
}
