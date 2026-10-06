// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;

namespace SAM.Core.Grasshopper
{
    public class ConnectionSnapshot
    {
        public GH_ParameterSide Side;
        public string ParamName;
        public string ParamNickName;
        public int ParamIndex;
        public GH_ParamAccess Access;
        public string ParamTypeName;
        public IGH_Param PeerParam;
        public string PeerParamName;
        public Guid PeerParamInstanceGuid;
        public string PeerComponentName;
        public string PeerComponentNickName;
        public Guid PeerComponentInstanceGuid;
    }
}
