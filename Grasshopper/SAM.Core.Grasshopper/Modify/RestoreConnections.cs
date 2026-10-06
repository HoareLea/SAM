// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static void RestoreConnections(GH_SAMComponent gH_SAMComponent, List<ParamConnection> connections, out Log log)
        {
            log = new Log();

            if (gH_SAMComponent == null || connections == null || connections.Count == 0)
            {
                return;
            }

            List<IGH_Param> outputParams = gH_SAMComponent.Params?.Output;
            List<IGH_Param> inputParams = gH_SAMComponent.Params?.Input;

            Dictionary<string, IGH_Param> newOutputParams = new Dictionary<string, IGH_Param>();
            Dictionary<string, IGH_Param> newInputParams = new Dictionary<string, IGH_Param>();

            if (outputParams != null)
            {
                foreach (IGH_Param param in outputParams)
                {
                    if (param != null)
                    {
                        newOutputParams[param.Name] = param;
                    }
                }
            }

            if (inputParams != null)
            {
                foreach (IGH_Param param in inputParams)
                {
                    if (param != null)
                    {
                        newInputParams[param.Name] = param;
                    }
                }
            }

            foreach (ParamConnection connection in connections)
            {
                Dictionary<string, IGH_Param> targetDict = connection.Side == GH_ParameterSide.Output ? newOutputParams : newInputParams;

                if (!targetDict.TryGetValue(connection.ParamName, out IGH_Param newParam))
                {
                    if (connection.ConnectedParams.Count > 0)
                    {
                        string sideLabel = connection.Side == GH_ParameterSide.Output ? "output" : "input";
                        log.Add(new LogRecord("  DROPPED: {0} {1} '{2}' not in new version ({3} {4} lost). Manual reconnect needed.",
                            LogRecordType.Warning, gH_SAMComponent.Name, sideLabel, connection.ParamName,
                            connection.ConnectedParams.Count, connection.ConnectedParams.Count == 1 ? "connection" : "connections"));

                        foreach (IGH_Param cp in connection.ConnectedParams)
                        {
                            if (cp != null)
                            {
                                string dest = "?";
                                try { dest = cp.Attributes?.ToString() ?? cp.Name; } catch { dest = cp.Name; }
                                log.Add(new LogRecord("    was connected to: {0}", LogRecordType.Warning, dest));
                            }
                        }
                    }
                    continue;
                }

                foreach (IGH_Param connectedParam in connection.ConnectedParams)
                {
                    if (connectedParam == null)
                    {
                        continue;
                    }

                    try
                    {
                        if (connection.Side == GH_ParameterSide.Output)
                        {
                            connectedParam.AddSource(newParam);
                        }
                        else
                        {
                            newParam.AddSource(connectedParam);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Add(new LogRecord("  Warning: failed to reconnect '{0}' → '{1}': {2}",
                            LogRecordType.Warning, connection.ParamName,
                            connectedParam.Name ?? "(unnamed)", ex.Message));
                    }
                }
            }
        }
    }
}
