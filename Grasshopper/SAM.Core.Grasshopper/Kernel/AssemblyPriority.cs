// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;

namespace SAM.Core.Grasshopper
{
    public class AssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("SAM", Resources.SAM_Small);
            Instances.ComponentServer.AddCategorySymbolName("SAM", 'S');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
