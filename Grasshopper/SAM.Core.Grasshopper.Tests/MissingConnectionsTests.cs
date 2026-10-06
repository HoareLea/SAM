// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SAM.Core.Grasshopper.Tests
{
    [TestFixture]
    public class MissingConnectionsTests
    {
        [Test]
        public void MissingConnections_WiresRestored_ReturnsEmpty()
        {
            TestUpdatableComponent component_Old = new TestUpdatableComponent();
            component_Old.CreateAttributes();
            Param_String source = TestDocument.CreateFloatingParam("source", 0, 100);
            Param_String recipient = TestDocument.CreateFloatingParam("recipient", 300, 100);
            component_Old.Params.Input[0].AddSource(source);
            recipient.AddSource(component_Old.Params.Output[0]);

            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component_Old);
            Assert.Equal(2, snapshots.Count);

            TestUpdatableComponent component_New = new TestUpdatableComponent();
            component_New.CreateAttributes();
            component_New.Params.Input[0].AddSource(source);
            recipient.AddSource(component_New.Params.Output[0]);

            List<ManualReconnectionWire> missing = Query.MissingConnections(component_New, snapshots);

            Assert.Empty(missing);
        }

        [Test]
        public void MissingConnections_InputWireNotRestored_ReportsWire()
        {
            TestUpdatableComponent component_Old = new TestUpdatableComponent();
            component_Old.CreateAttributes();
            Param_String source = TestDocument.CreateFloatingParam("source", 0, 100);
            component_Old.Params.Input[0].AddSource(source);

            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component_Old);

            TestUpdatableComponent component_New = new TestUpdatableComponent();
            component_New.CreateAttributes();

            List<ManualReconnectionWire> missing = Query.MissingConnections(component_New, snapshots);

            Assert.Single(missing);
            Assert.Equal(GH_ParameterSide.Input, missing[0].Side);
            Assert.Equal("a", missing[0].ParamName);
            Assert.Equal("source", missing[0].PeerParamName);
            Assert.Contains("could not be restored", missing[0].Reason);
        }

        [Test]
        public void MissingConnections_ConnectedInputRemoved_ReportsMissing()
        {
            TestUpdatableComponent component_Old = new TestUpdatableComponent();
            component_Old.CreateAttributes();
            Param_String legacyInput = TestDocument.RegisterLegacyInput(component_Old, "legacyIn");
            Param_String source = TestDocument.CreateFloatingParam("source", 0, 100);
            legacyInput.AddSource(source);

            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component_Old);

            TestUpdatableComponent component_New = new TestUpdatableComponent();
            component_New.CreateAttributes();

            List<ManualReconnectionWire> missing = Query.MissingConnections(component_New, snapshots);

            Assert.Single(missing);
            Assert.Equal(GH_ParameterSide.Input, missing[0].Side);
            Assert.Equal("legacyIn", missing[0].ParamName);
            Assert.Contains("does not exist", missing[0].Reason);
        }

        [Test]
        public void MissingConnections_ConnectedOutputRemoved_ReportsMissing()
        {
            TestUpdatableComponent component_Old = new TestUpdatableComponent();
            component_Old.CreateAttributes();
            Param_String legacyOutput = TestDocument.RegisterLegacyOutput(component_Old, "legacyOut");
            Param_String recipient = TestDocument.CreateFloatingParam("recipient", 300, 100);
            recipient.AddSource(legacyOutput);

            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component_Old);

            TestUpdatableComponent component_New = new TestUpdatableComponent();
            component_New.CreateAttributes();

            List<ManualReconnectionWire> missing = Query.MissingConnections(component_New, snapshots);

            Assert.Single(missing);
            Assert.Equal(GH_ParameterSide.Output, missing[0].Side);
            Assert.Equal("legacyOut", missing[0].ParamName);
            Assert.Contains("does not exist", missing[0].Reason);
        }

        [Test]
        public void MissingConnections_UnconnectedParameters_NotCaptured()
        {
            TestUpdatableComponent component = new TestUpdatableComponent();
            component.CreateAttributes();
            TestDocument.RegisterLegacyInput(component, "legacyIn");
            TestDocument.RegisterLegacyOutput(component, "legacyOut");

            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component);

            Assert.Empty(snapshots);
        }

        [Test]
        public void MissingConnections_SeveralMissingWires_AllReported()
        {
            TestUpdatableComponent component_Old = new TestUpdatableComponent();
            component_Old.CreateAttributes();
            Param_String legacyInput = TestDocument.RegisterLegacyInput(component_Old, "legacyIn");
            Param_String legacyOutput = TestDocument.RegisterLegacyOutput(component_Old, "legacyOut");
            Param_String source_1 = TestDocument.CreateFloatingParam("source_1", 0, 100);
            Param_String source_2 = TestDocument.CreateFloatingParam("source_2", 0, 200);
            Param_String recipient = TestDocument.CreateFloatingParam("recipient", 300, 100);
            legacyInput.AddSource(source_1);
            legacyInput.AddSource(source_2);
            recipient.AddSource(legacyOutput);

            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component_Old);
            Assert.Equal(3, snapshots.Count);

            TestUpdatableComponent component_New = new TestUpdatableComponent();
            component_New.CreateAttributes();

            List<ManualReconnectionWire> missing = Query.MissingConnections(component_New, snapshots);

            Assert.Equal(3, missing.Count);
            Assert.Equal(2, missing.Count(x => x.Side == GH_ParameterSide.Input));
            Assert.Equal(1, missing.Count(x => x.Side == GH_ParameterSide.Output));
        }

        [Test]
        public void MissingConnections_ReplacementUnavailable_ReportsAllWires()
        {
            TestUpdatableComponent component_Old = new TestUpdatableComponent();
            component_Old.CreateAttributes();
            Param_String source = TestDocument.CreateFloatingParam("source", 0, 100);
            component_Old.Params.Input[0].AddSource(source);

            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component_Old);

            List<ManualReconnectionWire> missing = Query.MissingConnections(null, snapshots);

            Assert.Single(missing);
            Assert.Contains("not available", missing[0].Reason);
        }

        [Test]
        public void SortManualReconnectionIssues_OrdersByPivotThenNameThenGuid()
        {
            ManualReconnectionIssue issue_1 = new ManualReconnectionIssue { ComponentName = "B", PivotX = 100, PivotY = 100, ComponentInstanceGuid = Guid.NewGuid() };
            ManualReconnectionIssue issue_2 = new ManualReconnectionIssue { ComponentName = "A", PivotX = 200, PivotY = 100, ComponentInstanceGuid = Guid.NewGuid() };
            ManualReconnectionIssue issue_3 = new ManualReconnectionIssue { ComponentName = "C", PivotX = 0, PivotY = 50, ComponentInstanceGuid = Guid.NewGuid() };
            ManualReconnectionIssue issue_4 = new ManualReconnectionIssue { ComponentName = "A", PivotX = 200, PivotY = 100, ComponentInstanceGuid = Guid.Empty };

            List<ManualReconnectionIssue> issues = new List<ManualReconnectionIssue> { issue_1, issue_2, issue_3, issue_4 };
            Modify.SortManualReconnectionIssues(issues);

            Assert.Same(issue_3, issues[0]);
            Assert.Same(issue_1, issues[1]);
            Assert.Same(issue_4, issues[2]);
            Assert.Same(issue_2, issues[3]);
        }

        [Test]
        public void ManualReconnectionIssue_ToText_ContainsDetails()
        {
            Guid guid = Guid.NewGuid();
            ManualReconnectionIssue issue = new ManualReconnectionIssue
            {
                ComponentInstanceGuid = guid,
                ComponentName = "Create Analytical Model",
                ComponentNickName = "Model"
            };
            issue.Wires.Add(new ManualReconnectionWire { Side = GH_ParameterSide.Input, ParamName = "Weather" });
            issue.Wires.Add(new ManualReconnectionWire { Side = GH_ParameterSide.Input, ParamName = "Systems" });
            issue.Wires.Add(new ManualReconnectionWire { Side = GH_ParameterSide.Output, ParamName = "Analytical Model" });

            string text = issue.ToText(1);

            Assert.StartsWith("[01] Create Analytical Model (Model)", text);
            Assert.Contains("3 missing wires", text);
            Assert.Contains("Inputs: Weather, Systems", text);
            Assert.Contains("Outputs: Analytical Model", text);
            Assert.Contains(guid.ToString(), text);
        }

        [Test]
        public void ManualReconnectionIssue_Summary_ReflectsSides()
        {
            ManualReconnectionIssue issue_Inputs = new ManualReconnectionIssue();
            issue_Inputs.Wires.Add(new ManualReconnectionWire { Side = GH_ParameterSide.Input, ParamName = "a" });
            Assert.Equal("1 missing input", issue_Inputs.Summary);

            ManualReconnectionIssue issue_Outputs = new ManualReconnectionIssue();
            issue_Outputs.Wires.Add(new ManualReconnectionWire { Side = GH_ParameterSide.Output, ParamName = "x" });
            issue_Outputs.Wires.Add(new ManualReconnectionWire { Side = GH_ParameterSide.Output, ParamName = "y" });
            Assert.Equal("2 missing outputs", issue_Outputs.Summary);

            ManualReconnectionIssue issue_Failed = new ManualReconnectionIssue { ReplacementFailed = true };
            Assert.Equal("replacement failed", issue_Failed.Summary);
        }

        [Test]
        public void NavigateTo_NullDocument_ReturnsFalse()
        {
            bool navigated = Modify.NavigateTo(null, Guid.NewGuid());

            Assert.False(navigated);
        }
    }
}
