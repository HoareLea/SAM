// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryInvokeRuntimeMethod<T>(object @object, string methodName, out T result, params object[] parameters)
        {
            return TryInvokeMethod(@object, @object?.GetType().GetRuntimeMethods(), methodName, out result, parameters);
        }
    }
}
