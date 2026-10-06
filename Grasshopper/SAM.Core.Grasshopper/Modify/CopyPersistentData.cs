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

                        //A freshly-constructed replacement can already carry its own baked-in default
                        //persistent data (e.g. a param whose registration calls SetPersistentData(...)),
                        //so appending the old param's values one at a time (AddPersistentData(T), looped)
                        //would double up: defaults, then the old values, instead of just the old values.
                        //SetPersistentData(GH_Structure<T>) replaces the target's tree wholesale - correct
                        //regardless of what the replacement started with, and it hands over the old
                        //param's branch structure intact rather than flattening it through repeated
                        //single-item adds.
                        System.Reflection.MethodInfo setMethod = type.GetMethod("SetPersistentData", new[] { persistentData.GetType() });
                        if (setMethod == null)
                        {
                            break;
                        }

                        setMethod.Invoke(newParam, new[] { persistentData });
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
