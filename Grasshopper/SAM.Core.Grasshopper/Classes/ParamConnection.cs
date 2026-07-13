// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public class ParamConnection
    {
        public GH_ParameterSide Side;
        public string ParamName;
        public List<IGH_Param> ConnectedParams = new List<IGH_Param>();
    }
}
