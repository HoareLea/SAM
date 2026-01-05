// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors


namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryInvokeDeclaredMethod<T>(dynamic @object, string methodName, out T result, params object[] parameters)
        {
            return TryInvokeMethod<T>(@object, @object?.GetType().DeclaredMethods, methodName, out result, parameters);
        }
    }
}
