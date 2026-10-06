// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;

namespace SAM.Core.Grasshopper
{
    public class ManualReconnectionWire
    {
        public GH_ParameterSide Side;
        public string ParamName;
        public string ParamNickName;
        public int ParamIndex;
        public GH_ParamAccess Access;
        public string ParamTypeName;
        public string PeerParamName;
        public Guid PeerParamInstanceGuid;
        public string PeerComponentName;
        public string PeerComponentNickName;
        public Guid PeerComponentInstanceGuid;
        public string Reason;

        public ManualReconnectionWire()
        {
        }

        public ManualReconnectionWire(ConnectionSnapshot connectionSnapshot, string reason)
        {
            if (connectionSnapshot != null)
            {
                Side = connectionSnapshot.Side;
                ParamName = connectionSnapshot.ParamName;
                ParamNickName = connectionSnapshot.ParamNickName;
                ParamIndex = connectionSnapshot.ParamIndex;
                Access = connectionSnapshot.Access;
                ParamTypeName = connectionSnapshot.ParamTypeName;
                PeerParamName = connectionSnapshot.PeerParamName;
                PeerParamInstanceGuid = connectionSnapshot.PeerParamInstanceGuid;
                PeerComponentName = connectionSnapshot.PeerComponentName;
                PeerComponentNickName = connectionSnapshot.PeerComponentNickName;
                PeerComponentInstanceGuid = connectionSnapshot.PeerComponentInstanceGuid;
            }

            Reason = reason;
        }
    }
}
