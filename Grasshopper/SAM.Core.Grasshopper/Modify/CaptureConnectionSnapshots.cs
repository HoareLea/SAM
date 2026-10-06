// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static List<ConnectionSnapshot> CaptureConnectionSnapshots(GH_SAMComponent gH_SAMComponent)
        {
            List<ConnectionSnapshot> result = new List<ConnectionSnapshot>();

            if (gH_SAMComponent == null)
            {
                return result;
            }

            List<IGH_Param> inputParams = gH_SAMComponent.Params?.Input;
            if (inputParams != null)
            {
                for (int i = 0; i < inputParams.Count; i++)
                {
                    IGH_Param param = inputParams[i];
                    if (param?.Sources == null || param.Sources.Count == 0)
                    {
                        continue;
                    }

                    foreach (IGH_Param source in param.Sources)
                    {
                        result.Add(CreateConnectionSnapshot(GH_ParameterSide.Input, param, i, source));
                    }
                }
            }

            List<IGH_Param> outputParams = gH_SAMComponent.Params?.Output;
            if (outputParams != null)
            {
                for (int i = 0; i < outputParams.Count; i++)
                {
                    IGH_Param param = outputParams[i];
                    if (param?.Recipients == null || param.Recipients.Count == 0)
                    {
                        continue;
                    }

                    foreach (IGH_Param recipient in param.Recipients)
                    {
                        result.Add(CreateConnectionSnapshot(GH_ParameterSide.Output, param, i, recipient));
                    }
                }
            }

            return result;
        }

        private static ConnectionSnapshot CreateConnectionSnapshot(GH_ParameterSide side, IGH_Param param, int paramIndex, IGH_Param peerParam)
        {
            ConnectionSnapshot result = new ConnectionSnapshot
            {
                Side = side,
                ParamName = param?.Name,
                ParamNickName = param?.NickName,
                ParamIndex = paramIndex,
                Access = param == null ? GH_ParamAccess.item : param.Access,
                ParamTypeName = param?.TypeName,
                PeerParam = peerParam,
                PeerParamName = peerParam?.Name,
                PeerParamInstanceGuid = peerParam == null ? Guid.Empty : peerParam.InstanceGuid
            };

            IGH_DocumentObject peerDocumentObject = peerParam?.Attributes?.GetTopLevel?.DocObject;
            if (peerDocumentObject != null)
            {
                result.PeerComponentName = peerDocumentObject.Name;
                result.PeerComponentNickName = peerDocumentObject.NickName;
                result.PeerComponentInstanceGuid = peerDocumentObject.InstanceGuid;
            }
            else if (peerParam != null)
            {
                result.PeerComponentName = peerParam.Name;
                result.PeerComponentNickName = peerParam.NickName;
                result.PeerComponentInstanceGuid = peerParam.InstanceGuid;
            }

            return result;
        }
    }
}
