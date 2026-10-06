// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static List<GH_SAMComponent> UpdateComponents(GH_Document gH_Document, out Log log)
        {
            return UpdateComponents(gH_Document, out log, out List<ManualReconnectionIssue> manualReconnectionIssues);
        }

        public static List<GH_SAMComponent> UpdateComponents(GH_Document gH_Document, out Log log, out List<ManualReconnectionIssue> manualReconnectionIssues)
        {
            log = new Log();
            manualReconnectionIssues = new List<ManualReconnectionIssue>();

            if (gH_Document == null)
            {
                return null;
            }

            IList<IGH_DocumentObject> gH_DocumentObjects = gH_Document.Objects;
            if (gH_DocumentObjects == null || gH_DocumentObjects.Count == 0)
            {
                return null;
            }

            List<GH_SAMComponent> gH_SAMComponents = new List<GH_SAMComponent>();
            foreach (IGH_DocumentObject gH_DocumentObject in gH_DocumentObjects)
            {
                if (!(gH_DocumentObject is GH_SAMComponent gH_SAMComponent))
                {
                    continue;
                }

                if (gH_SAMComponent.Obsolete)
                {
                    gH_SAMComponents.Add(gH_SAMComponent);
                }
            }

            if (gH_SAMComponents.Count == 0)
            {
                log.Add(new LogRecord("No obsolete components found.", LogRecordType.Message));
                return new List<GH_SAMComponent>();
            }

            return UpdateComponents(gH_SAMComponents, gH_Document, out log, out manualReconnectionIssues);
        }

        public static List<GH_SAMComponent> UpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents, out Log log)
        {
            return UpdateComponents(gH_SAMComponents, out log, out List<ManualReconnectionIssue> manualReconnectionIssues);
        }

        public static List<GH_SAMComponent> UpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents, out Log log, out List<ManualReconnectionIssue> manualReconnectionIssues)
        {
            log = new Log();
            manualReconnectionIssues = new List<ManualReconnectionIssue>();

            if (gH_SAMComponents == null || !gH_SAMComponents.Any())
            {
                return null;
            }

            GH_Document gH_Document = gH_SAMComponents.FirstOrDefault()?.OnPingDocument();
            if (gH_Document == null)
            {
                log.Add(new LogRecord("Could not access document.", LogRecordType.Error));
                return null;
            }

            return UpdateComponents(gH_SAMComponents, gH_Document, out log, out manualReconnectionIssues);
        }

        private static List<GH_SAMComponent> UpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents, GH_Document gH_Document, out Log log)
        {
            return UpdateComponents(gH_SAMComponents, gH_Document, out log, out List<ManualReconnectionIssue> manualReconnectionIssues);
        }

        private static List<GH_SAMComponent> UpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents, GH_Document gH_Document, out Log log, out List<ManualReconnectionIssue> manualReconnectionIssues)
        {
            log = new Log();
            manualReconnectionIssues = new List<ManualReconnectionIssue>();

            if (gH_SAMComponents == null || !gH_SAMComponents.Any())
            {
                return null;
            }

            if (gH_Document == null)
            {
                log.Add(new LogRecord("Document is null.", LogRecordType.Error));
                return null;
            }

            GH_SAMComponent[] components = gH_SAMComponents.ToArray();

            int obsoleteCount = components.Count(c => c != null && c.Obsolete);
            log.Add(new LogRecord("Update started. Targeting {0} component(s), {1} are obsolete.",
                LogRecordType.Message, components.Length, obsoleteCount));

            int updated = 0;
            int failed = 0;
            int warnings = 0;
            List<GH_SAMComponent> result = new List<GH_SAMComponent>();
            List<GH_SAMComponent> oldComponents = new List<GH_SAMComponent>();

            try
            {
                gH_Document.UndoUtil.RecordEvent("Update Obsolete Components");
            }
            catch
            {
                log.Add(new LogRecord("Undo grouping not available — updates will be individually undoable.", LogRecordType.Warning));
                warnings++;
            }

            foreach (GH_SAMComponent gH_SAMComponent in components)
            {
                if (gH_SAMComponent == null)
                {
                    continue;
                }

                if (!gH_SAMComponent.Obsolete)
                {
                    continue;
                }

                try
                {
                    GH_SAMComponent gH_SAMComponent_New = DuplicateComponent(gH_SAMComponent, out Log log_Temp, out List<ManualReconnectionWire> manualReconnectionWires);
                    if (gH_SAMComponent_New == null)
                    {
                        failed++;
                        if (log_Temp != null)
                        {
                            log.AddRange(log_Temp);
                        }

                        ManualReconnectionIssue manualReconnectionIssue = CreateManualReconnectionIssue(gH_SAMComponent, null, true);
                        if (manualReconnectionIssue != null)
                        {
                            manualReconnectionIssues.Add(manualReconnectionIssue);
                        }

                        continue;
                    }

                    result.Add(gH_SAMComponent_New);
                    oldComponents.Add(gH_SAMComponent);
                    updated++;

                    if (log_Temp != null)
                    {
                        log.AddRange(log_Temp);
                    }

                    if (manualReconnectionWires != null && manualReconnectionWires.Count != 0)
                    {
                        ManualReconnectionIssue manualReconnectionIssue = CreateManualReconnectionIssue(gH_SAMComponent_New, manualReconnectionWires, false);
                        if (manualReconnectionIssue != null)
                        {
                            manualReconnectionIssues.Add(manualReconnectionIssue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    log.Add(new LogRecord("  Failed to update {0}: {1}", LogRecordType.Error, gH_SAMComponent.Name, ex.Message));

                    ManualReconnectionIssue manualReconnectionIssue = CreateManualReconnectionIssue(gH_SAMComponent, null, true);
                    if (manualReconnectionIssue != null)
                    {
                        manualReconnectionIssues.Add(manualReconnectionIssue);
                    }
                }
            }

            foreach (GH_SAMComponent oldComponent in oldComponents)
            {
                gH_Document.RemoveObject(oldComponent, false);
            }

            SortManualReconnectionIssues(manualReconnectionIssues);

            log.Add(new LogRecord("Update completed: {0} updated, {1} failed, {2} warning(s).",
                LogRecordType.Message, updated, failed, warnings));

            if (manualReconnectionIssues.Count != 0)
            {
                log.Add(new LogRecord("{0} component(s) require manual wire reconnection.", LogRecordType.Warning, manualReconnectionIssues.Count));
            }

            if (result.Count == 0)
            {
                return null;
            }

            return result;
        }

        private static ManualReconnectionIssue CreateManualReconnectionIssue(GH_SAMComponent gH_SAMComponent, List<ManualReconnectionWire> manualReconnectionWires, bool replacementFailed)
        {
            if (gH_SAMComponent == null)
            {
                return null;
            }

            System.Drawing.PointF pivot = gH_SAMComponent.Attributes?.Pivot ?? System.Drawing.PointF.Empty;

            ManualReconnectionIssue result = new ManualReconnectionIssue
            {
                ComponentInstanceGuid = gH_SAMComponent.InstanceGuid,
                ComponentName = gH_SAMComponent.Name,
                ComponentNickName = gH_SAMComponent.NickName,
                PivotX = pivot.X,
                PivotY = pivot.Y,
                ReplacementFailed = replacementFailed
            };

            if (manualReconnectionWires != null)
            {
                result.Wires.AddRange(manualReconnectionWires);
            }

            return result;
        }

        public static void SortManualReconnectionIssues(List<ManualReconnectionIssue> manualReconnectionIssues)
        {
            if (manualReconnectionIssues == null)
            {
                return;
            }

            manualReconnectionIssues.Sort((x, y) =>
            {
                if (x == null || y == null)
                {
                    return x == null ? (y == null ? 0 : 1) : -1;
                }

                int comparison = x.PivotY.CompareTo(y.PivotY);
                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = x.PivotX.CompareTo(y.PivotX);
                if (comparison != 0)
                {
                    return comparison;
                }

                comparison = string.Compare(x.ComponentName, y.ComponentName, StringComparison.Ordinal);
                if (comparison != 0)
                {
                    return comparison;
                }

                return x.ComponentInstanceGuid.CompareTo(y.ComponentInstanceGuid);
            });
        }

        public static List<GH_SAMComponent> PreviewUpdateComponents(GH_Document gH_Document, out Log log)
        {
            log = new Log();

            if (gH_Document == null)
            {
                return null;
            }

            IList<IGH_DocumentObject> gH_DocumentObjects = gH_Document.Objects;
            if (gH_DocumentObjects == null || gH_DocumentObjects.Count == 0)
            {
                log.Add(new LogRecord("No components found in document.", LogRecordType.Message));
                return null;
            }

            List<GH_SAMComponent> gH_SAMComponents = new List<GH_SAMComponent>();
            foreach (IGH_DocumentObject gH_DocumentObject in gH_DocumentObjects)
            {
                if (!(gH_DocumentObject is GH_SAMComponent gH_SAMComponent))
                {
                    continue;
                }

                if (gH_SAMComponent.Obsolete)
                {
                    gH_SAMComponents.Add(gH_SAMComponent);
                }
            }

            if (gH_SAMComponents.Count == 0)
            {
                log.Add(new LogRecord("No obsolete components found.", LogRecordType.Message));
                return null;
            }

            return PreviewUpdateComponents(gH_SAMComponents, gH_Document, out log);
        }

        public static List<GH_SAMComponent> PreviewUpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents, out Log log)
        {
            log = new Log();
            if (gH_SAMComponents == null || !gH_SAMComponents.Any())
            {
                return null;
            }

            GH_Document gH_Document = gH_SAMComponents.FirstOrDefault()?.OnPingDocument();
            if (gH_Document == null)
            {
                log.Add(new LogRecord("Could not access document.", LogRecordType.Error));
                return null;
            }

            return PreviewUpdateComponents(gH_SAMComponents, gH_Document, out log);
        }

        private static List<GH_SAMComponent> PreviewUpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents, GH_Document gH_Document, out Log log)
        {
            log = new Log();

            if (gH_SAMComponents == null || !gH_SAMComponents.Any())
            {
                return null;
            }

            if (gH_Document == null)
            {
                log.Add(new LogRecord("Document is null.", LogRecordType.Error));
                return null;
            }

            GH_SAMComponent[] components = gH_SAMComponents.ToArray();

            log.Add(new LogRecord("--- DRY RUN --- Would update {0} component(s):", LogRecordType.Message, components.Length));

            int connectionsAtRisk = 0;
            List<GH_SAMComponent> result = new List<GH_SAMComponent>();

            foreach (GH_SAMComponent gH_SAMComponent in components)
            {
                if (gH_SAMComponent == null || !gH_SAMComponent.Obsolete)
                {
                    continue;
                }

                ObsoleteSeverity severity = gH_SAMComponent.ObsoleteSeverity;
                string severityLabel = severity == ObsoleteSeverity.Breaking ? " [BREAKING]" :
                                       severity == ObsoleteSeverity.Advisory ? " [patch]" : "";

                log.Add(new LogRecord("  {0}: {1} → {2}{3}",
                    LogRecordType.Message, gH_SAMComponent.Name, gH_SAMComponent.ComponentVersion,
                    gH_SAMComponent.LatestComponentVersion, severityLabel));

                result.Add(gH_SAMComponent);

                List<IGH_Param> outputParams = gH_SAMComponent.Params?.Output;
                List<IGH_Param> inputParams = gH_SAMComponent.Params?.Input;

                if (outputParams != null)
                {
                    foreach (IGH_Param param in outputParams)
                    {
                        if (param != null && param.Recipients != null && param.Recipients.Count > 0)
                        {
                            connectionsAtRisk += param.Recipients.Count;
                        }
                    }
                }

                if (inputParams != null)
                {
                    foreach (IGH_Param param in inputParams)
                    {
                        if (param != null && param.Sources != null && param.Sources.Count > 0)
                        {
                            connectionsAtRisk += param.Sources.Count;
                        }
                    }
                }
            }

            if (connectionsAtRisk > 0)
            {
                log.Add(new LogRecord("  {0} total connection(s) will be rewired.", LogRecordType.Message, connectionsAtRisk));
            }

            log.Add(new LogRecord("--- DRY RUN COMPLETE --- No changes were made to the document.", LogRecordType.Message));

            return result;
        }

        public static List<GH_SAMComponent> DuplicateComponents(
            IEnumerable<GH_SAMComponent> gH_SAMComponents,
            GH_Document gH_Document,
            out Log log,
            out List<GH_SAMComponent> oldComponents)
        {
            log = new Log();
            oldComponents = new List<GH_SAMComponent>();

            if (gH_Document == null)
            {
                log.Add(new LogRecord("Document is null.", LogRecordType.Error));
                return null;
            }

            List<GH_SAMComponent> targets;
            if (gH_SAMComponents != null && gH_SAMComponents.Any())
            {
                targets = gH_SAMComponents.ToList();
            }
            else
            {
                targets = new List<GH_SAMComponent>();
                IList<IGH_DocumentObject> gH_DocumentObjects = gH_Document.Objects;
                if (gH_DocumentObjects != null)
                {
                    foreach (IGH_DocumentObject gH_DocumentObject in gH_DocumentObjects)
                    {
                        if (gH_DocumentObject is GH_SAMComponent gH_SAMComponent && gH_SAMComponent.Obsolete)
                        {
                            targets.Add(gH_SAMComponent);
                        }
                    }
                }
            }

            if (targets.Count == 0)
            {
                log.Add(new LogRecord("No obsolete components found.", LogRecordType.Message));
                return new List<GH_SAMComponent>();
            }

            log.Add(new LogRecord("Update started. Found {0} obsolete component(s).", LogRecordType.Message, targets.Count));

            List<GH_SAMComponent> result = new List<GH_SAMComponent>();

            foreach (GH_SAMComponent gH_SAMComponent in targets)
            {
                if (gH_SAMComponent == null)
                {
                    continue;
                }

                if (!gH_SAMComponent.Obsolete)
                {
                    continue;
                }

                try
                {
                    GH_SAMComponent gH_SAMComponent_New = DuplicateComponent(gH_SAMComponent, out Log log_Temp);
                    if (gH_SAMComponent_New == null)
                    {
                        log.Add(new LogRecord("  Failed to update {0}", LogRecordType.Error, gH_SAMComponent.Name));
                        if (log_Temp != null)
                        {
                            log.AddRange(log_Temp);
                        }
                        continue;
                    }

                    result.Add(gH_SAMComponent_New);
                    oldComponents.Add(gH_SAMComponent);

                    if (log_Temp != null)
                    {
                        log.AddRange(log_Temp);
                    }
                }
                catch (Exception ex)
                {
                    log.Add(new LogRecord("  Failed to update {0}: {1}", LogRecordType.Error, gH_SAMComponent.Name, ex.Message));
                }
            }

            log.Add(new LogRecord("Components duplicated: {0} new created, {1} pending removal.",
                LogRecordType.Message, result.Count, oldComponents.Count));

            return result;
        }
    }
}
