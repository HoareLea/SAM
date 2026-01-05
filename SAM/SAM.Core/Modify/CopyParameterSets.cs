// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static List<ParameterSet> CopyParameterSets(this SAMObject object_Source, SAMObject object_Destination)
        {
            if (object_Source == null || object_Destination == null)
            {
                return null;
            }

            List<ParameterSet> result = object_Source.GetParameterSets();
            if (result == null || result.Count == 0)
            {
                return result;
            }

            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i]?.Clone();

                object_Destination.Add(result[i]);
            }
            return result;
        }
    }
}
