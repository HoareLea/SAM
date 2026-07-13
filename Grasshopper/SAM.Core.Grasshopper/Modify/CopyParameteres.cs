// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        [Obsolete("Use CopyParameters instead.", false)]
        public static void CopyParameteres(GH_ParameterSide parameterSide, GH_SAMComponent gH_SAMComponent_From, GH_SAMComponent gH_SAMComponent_To)
        {
            CopyParameters(parameterSide, gH_SAMComponent_From, gH_SAMComponent_To);
        }
    }
}
