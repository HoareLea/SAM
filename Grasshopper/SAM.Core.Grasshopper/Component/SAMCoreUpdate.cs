// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreUpdate : GH_SAMVariableOutputParameterComponent
    {
        public override Guid ComponentGuid => new Guid("a89bfee3-3a3c-4d29-9c7a-64073724eddc");

        public override string LatestComponentVersion => "1.1.0";

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_String param_Components;
                param_Components = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_components", NickName = "_components", Description = "Component names, nicknames, or GUID strings (optional). If empty, scans entire document.", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(param_Components, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_DryRun;
                param_DryRun = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_dryRun", NickName = "_dryRun", Description = "Preview mode — reports changes without applying them", Access = GH_ParamAccess.item, Optional = true };
                param_DryRun.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_DryRun, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;
                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run Update", Access = GH_ParamAccess.item, Optional = false };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooLogParam() { Name = "log", NickName = "log", Description = "Log", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "succeeded", NickName = "succeeded", Description = "Succeeded", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "manualReconnection", NickName = "manual", Description = "Updated components that require manual wire reconnection. Right-click this component to navigate to each issue.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        private List<ManualReconnectionIssue> manualReconnectionIssues = new List<ManualReconnectionIssue>();
        private int currentManualReconnectionIndex = -1;

        public SAMCoreUpdate()
          : base("SAMCore.Update", "SAMCore.Update",
              "Updates Grasshopper components to the latest version",
              "SAM", "SAM")
        {
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown toolStripDropDown)
        {
            Menu_AppendItem(toolStripDropDown, "Update", Menu_Update);
            AppendManualReconnectionMenuItems(toolStripDropDown);
        }

        private void AppendManualReconnectionMenuItems(ToolStripDropDown toolStripDropDown)
        {
            int count = manualReconnectionIssues == null ? 0 : manualReconnectionIssues.Count;

            ToolStripMenuItem toolStripMenuItem = Menu_AppendItem(toolStripDropDown, string.Format("Manual reconnections ({0})", count));
            if (toolStripMenuItem == null)
            {
                return;
            }

            if (count == 0)
            {
                toolStripMenuItem.Enabled = false;
                return;
            }

            Menu_AppendItem(toolStripMenuItem.DropDown, "Next issue", Menu_NextIssue);
            Menu_AppendItem(toolStripMenuItem.DropDown, "Previous issue", Menu_PreviousIssue);
            Menu_AppendItem(toolStripMenuItem.DropDown, "Select all issues", Menu_SelectAllIssues);
            Menu_AppendSeparator(toolStripMenuItem.DropDown);

            for (int i = 0; i < manualReconnectionIssues.Count; i++)
            {
                ManualReconnectionIssue manualReconnectionIssue = manualReconnectionIssues[i];
                if (manualReconnectionIssue == null)
                {
                    continue;
                }

                ToolStripMenuItem issueToolStripMenuItem = Menu_AppendItem(toolStripMenuItem.DropDown, string.Format("{0:00} — {1}", i + 1, manualReconnectionIssue.ToString()), Menu_IssueClicked);
                if (issueToolStripMenuItem != null)
                {
                    issueToolStripMenuItem.Tag = i;
                }
            }
        }

        private void Menu_Update(object sender, EventArgs e)
        {
            Modify.UpdateComponents(OnPingDocument(), out Log log, out List<ManualReconnectionIssue> manualReconnectionIssues_Temp);
            SetManualReconnectionIssues(manualReconnectionIssues_Temp);
        }

        private void Menu_IssueClicked(object sender, EventArgs e)
        {
            if (!((sender as ToolStripMenuItem)?.Tag is int index))
            {
                return;
            }

            if (manualReconnectionIssues == null || index < 0 || index >= manualReconnectionIssues.Count)
            {
                return;
            }

            currentManualReconnectionIndex = index;
            NavigateToIssue(manualReconnectionIssues[index]);
        }

        private void Menu_NextIssue(object sender, EventArgs e)
        {
            NavigateToOffsetIssue(1);
        }

        private void Menu_PreviousIssue(object sender, EventArgs e)
        {
            NavigateToOffsetIssue(-1);
        }

        private void Menu_SelectAllIssues(object sender, EventArgs e)
        {
            if (manualReconnectionIssues == null || manualReconnectionIssues.Count == 0)
            {
                return;
            }

            List<Guid> instanceGuids = new List<Guid>();
            foreach (ManualReconnectionIssue manualReconnectionIssue in manualReconnectionIssues)
            {
                if (manualReconnectionIssue == null)
                {
                    continue;
                }

                instanceGuids.Add(manualReconnectionIssue.ComponentInstanceGuid);
            }

            Modify.NavigateTo(OnPingDocument(), instanceGuids);
        }

        private void NavigateToOffsetIssue(int offset)
        {
            if (manualReconnectionIssues == null || manualReconnectionIssues.Count == 0)
            {
                return;
            }

            int count = manualReconnectionIssues.Count;
            currentManualReconnectionIndex = ((currentManualReconnectionIndex + offset) % count + count) % count;
            NavigateToIssue(manualReconnectionIssues[currentManualReconnectionIndex]);
        }

        private void NavigateToIssue(ManualReconnectionIssue manualReconnectionIssue)
        {
            if (manualReconnectionIssue == null)
            {
                return;
            }

            Modify.NavigateTo(OnPingDocument(), manualReconnectionIssue.ComponentInstanceGuid);
        }

        private void SetManualReconnectionIssues(List<ManualReconnectionIssue> manualReconnectionIssues)
        {
            this.manualReconnectionIssues = manualReconnectionIssues ?? new List<ManualReconnectionIssue>();
            currentManualReconnectionIndex = -1;
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            bool dryRun = false;
            index = Params.IndexOfInputParam("_dryRun");
            if (index != -1)
            {
                dataAccess.GetData(index, ref dryRun);
            }

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index != -1)
            {
                dataAccess.GetData(index, ref run);
            }

            if (!dryRun && !run)
            {
                return;
            }

            GH_Document gH_Document = OnPingDocument();
            if (gH_Document == null)
            {
                return;
            }

            List<GH_SAMComponent> targetComponents = ResolveTargetComponents(dataAccess, gH_Document);

            Log log;
            List<GH_SAMComponent> result;
            List<ManualReconnectionIssue> manualReconnectionIssues_Temp = new List<ManualReconnectionIssue>();

            if (dryRun)
            {
                if (targetComponents != null && targetComponents.Count > 0)
                {
                    result = Modify.PreviewUpdateComponents(targetComponents, out log);
                }
                else
                {
                    result = Modify.PreviewUpdateComponents(gH_Document, out log);
                }
            }
            else
            {
                if (targetComponents != null && targetComponents.Count > 0)
                {
                    result = Modify.UpdateComponents(targetComponents, out log, out manualReconnectionIssues_Temp);
                }
                else
                {
                    result = Modify.UpdateComponents(gH_Document, out log, out manualReconnectionIssues_Temp);
                }

                SetManualReconnectionIssues(manualReconnectionIssues_Temp);
            }

            index = Params.IndexOfOutputParam("log");
            if (index != -1)
            {
                dataAccess.SetData(index, log);
            }

            index = Params.IndexOfOutputParam("succeeded");
            if (index != -1)
            {
                dataAccess.SetData(index, !dryRun && result != null && result.Count != 0);
            }

            index = Params.IndexOfOutputParam("manualReconnection");
            if (index != -1)
            {
                List<string> manualReconnectionTexts = new List<string>();
                if (manualReconnectionIssues_Temp != null)
                {
                    for (int i = 0; i < manualReconnectionIssues_Temp.Count; i++)
                    {
                        if (manualReconnectionIssues_Temp[i] == null)
                        {
                            continue;
                        }

                        manualReconnectionTexts.Add(manualReconnectionIssues_Temp[i].ToText(i + 1));
                    }
                }

                dataAccess.SetDataList(index, manualReconnectionTexts);
            }

            if (!dryRun && manualReconnectionIssues_Temp != null && manualReconnectionIssues_Temp.Count != 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(
                    "{0} updated component(s) require manual wire reconnection. Use the manualReconnection output or right-click this component to navigate to each issue.",
                    manualReconnectionIssues_Temp.Count));
            }
        }

        private List<GH_SAMComponent> ResolveTargetComponents(IGH_DataAccess dataAccess, GH_Document gH_Document)
        {
            int index = Params.IndexOfInputParam("_components");
            if (index == -1)
            {
                return null;
            }

            List<string> names = new List<string>();
            if (!dataAccess.GetDataList(index, names) || names.Count == 0)
            {
                return null;
            }

            if (gH_Document == null)
            {
                return null;
            }

            List<GH_SAMComponent> result = new List<GH_SAMComponent>();
            IList<IGH_DocumentObject> allObjects = gH_Document.Objects;
            if (allObjects == null)
            {
                return null;
            }

            foreach (string search in names)
            {
                if (string.IsNullOrWhiteSpace(search))
                {
                    continue;
                }

                string trimmed = search.Trim();

                if (Guid.TryParse(trimmed, out Guid guid))
                {
                    foreach (IGH_DocumentObject obj in allObjects)
                    {
                        if (obj is GH_SAMComponent samComp && samComp.InstanceGuid == guid)
                        {
                            if (!result.Contains(samComp))
                            {
                                result.Add(samComp);
                            }
                            break;
                        }
                    }
                    continue;
                }

                foreach (IGH_DocumentObject obj in allObjects)
                {
                    if (obj is GH_SAMComponent samComp)
                    {
                        if (string.Equals(samComp.Name, trimmed, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(samComp.NickName, trimmed, StringComparison.OrdinalIgnoreCase) ||
                            samComp.Name.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!result.Contains(samComp))
                            {
                                result.Add(samComp);
                            }
                            break;
                        }
                    }
                }
            }

            return result.Count > 0 ? result : null;
        }
    }
}
