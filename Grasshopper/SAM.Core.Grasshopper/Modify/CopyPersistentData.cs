// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        internal static void CopyPersistentData(IGH_Param oldParam, IGH_Param newParam)
        {
            if (oldParam == null || newParam == null)
            {
                return;
            }

            if (oldParam.GetType() != newParam.GetType())
            {
                return;
            }

            try
            {
                Type type = oldParam.GetType();
                while (type != null && type != typeof(object))
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == "Grasshopper.Kernel.GH_PersistentParam`1")
                    {
                        System.Reflection.PropertyInfo persistentDataProp = type.GetProperty("PersistentData");
                        if (persistentDataProp == null)
                        {
                            break;
                        }

                        object persistentData = persistentDataProp.GetValue(oldParam);
                        if (persistentData == null)
                        {
                            break;
                        }

                        if (!(persistentData is System.Collections.IEnumerable enumerable))
                        {
                            break;
                        }

                        System.Reflection.MethodInfo addMethod = type.GetMethod("AddPersistentData");
                        if (addMethod == null)
                        {
                            break;
                        }

                        foreach (object item in enumerable)
                        {
                            addMethod.Invoke(newParam, new[] { item });
                        }
                        break;
                    }
                    type = type.BaseType;
                }
            }
            catch
            {
                // Best-effort: persistent data copy is non-critical
            }
        }
    }
}
