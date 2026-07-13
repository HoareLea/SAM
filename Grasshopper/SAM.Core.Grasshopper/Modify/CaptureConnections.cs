// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static List<ParamConnection> CaptureConnections(GH_SAMComponent gH_SAMComponent)
        {
            List<ParamConnection> result = new List<ParamConnection>();

            if (gH_SAMComponent == null)
            {
                return result;
            }

            List<IGH_Param> outputParams = gH_SAMComponent.Params?.Output;
            if (outputParams != null)
            {
                foreach (IGH_Param param in outputParams)
                {
                    if (param == null || param.Recipients == null || param.Recipients.Count == 0)
                    {
                        continue;
                    }

                    result.Add(new ParamConnection
                    {
                        Side = GH_ParameterSide.Output,
                        ParamName = param.Name,
                        ConnectedParams = new List<IGH_Param>(param.Recipients)
                    });
                }
            }

            List<IGH_Param> inputParams = gH_SAMComponent.Params?.Input;
            if (inputParams != null)
            {
                foreach (IGH_Param param in inputParams)
                {
                    if (param == null || param.Sources == null || param.Sources.Count == 0)
                    {
                        continue;
                    }

                    result.Add(new ParamConnection
                    {
                        Side = GH_ParameterSide.Input,
                        ParamName = param.Name,
                        ConnectedParams = new List<IGH_Param>(param.Sources)
                    });
                }
            }

            return result;
        }
    }
}
