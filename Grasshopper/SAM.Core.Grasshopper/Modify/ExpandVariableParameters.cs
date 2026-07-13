// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        internal static void ExpandVariableParameters(GH_SAMComponent gH_SAMComponent_From, GH_SAMComponent gH_SAMComponent_To)
        {
            if (!(gH_SAMComponent_To is GH_SAMVariableOutputParameterComponent variableOutput))
            {
                return;
            }

            try
            {
                ExpandSide(GH_ParameterSide.Input, gH_SAMComponent_From, gH_SAMComponent_To, variableOutput);
                ExpandSide(GH_ParameterSide.Output, gH_SAMComponent_From, gH_SAMComponent_To, variableOutput);
            }
            catch
            {
                // Best-effort: expansion failure is non-critical
            }
        }

        private static void ExpandSide(GH_ParameterSide side, GH_SAMComponent from, GH_SAMComponent to, GH_SAMVariableOutputParameterComponent variableOutput)
        {
            List<IGH_Param> fromParams = side == GH_ParameterSide.Output ? from.Params?.Output : from.Params?.Input;
            List<IGH_Param> toParams = side == GH_ParameterSide.Output ? to.Params?.Output : to.Params?.Input;

            if (fromParams == null || toParams == null || fromParams.Count == 0)
            {
                return;
            }

            HashSet<string> fromParamNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (IGH_Param param in fromParams)
            {
                if (param != null)
                {
                    fromParamNames.Add(param.Name);
                }
            }

            HashSet<string> toParamNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (IGH_Param param in toParams)
            {
                if (param != null)
                {
                    toParamNames.Add(param.Name);
                }
            }

            List<string> missingNames = new List<string>();
            foreach (string name in fromParamNames)
            {
                if (!toParamNames.Contains(name))
                {
                    missingNames.Add(name);
                }
            }

            if (missingNames.Count == 0)
            {
                return;
            }

            int attempts = 0;
            int maxAttempts = missingNames.Count * 3;
            while (missingNames.Count > 0 && attempts < maxAttempts)
            {
                attempts++;
                bool added = false;

                for (int i = toParams.Count - 1; i >= 0; i--)
                {
                    if (!variableOutput.CanInsertParameter(side, i))
                    {
                        continue;
                    }

                    IGH_Param newParam = null;
                    try
                    {
                        newParam = variableOutput.CreateParameter(side, i);
                    }
                    catch
                    {
                        continue;
                    }

                    if (newParam == null)
                    {
                        continue;
                    }

                    if (missingNames.Contains(newParam.Name))
                    {
                        toParams.Add(newParam);
                        missingNames.Remove(newParam.Name);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    break;
                }
            }
        }
    }
}
