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
    public class ManualReconnectionTests
    {
        private const string SkipReason = "GH_Document requires the Rhino native runtime; run these tests in a Rhino-enabled environment.";

        private static GH_Document CreateDocumentOrSkip()
        {
            GH_Document result = TestDocument.TryCreate();
            Skip.If(result == null, SkipReason);
            return result;
        }

        [Test]
        public void UpdateComponents_AllWiresRestored_NoIssues()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String source = TestDocument.AddFloatingParam(document, "source", 0, 100);
            Param_String recipient = TestDocument.AddFloatingParam(document, "recipient", 300, 100);

            component.Params.Input[0].AddSource(source);
            recipient.AddSource(component.Params.Output[0]);
            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.NotNull(updated);
            Assert.Single(updated);
            Assert.Empty(issues);

            GH_SAMComponent newComponent = updated[0];
            Assert.Contains(source, newComponent.Params.Input[0].Sources);
            Assert.Contains(newComponent.Params.Output[0], recipient.Sources);
            Assert.Null(document.FindObject(component.InstanceGuid, true));
        }

        [Test]
        public void UpdateComponents_ConnectedInputRemoved_ReportsMissingInput()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String legacyInput = TestDocument.RegisterLegacyInput(component, "legacyIn");
            Param_String source = TestDocument.AddFloatingParam(document, "source", 0, 100);

            legacyInput.AddSource(source);
            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.NotNull(updated);
            Assert.Single(issues);

            ManualReconnectionIssue issue = issues[0];
            Assert.False(issue.ReplacementFailed);
            Assert.Single(issue.Wires);
            Assert.Equal(GH_ParameterSide.Input, issue.Wires[0].Side);
            Assert.Equal("legacyIn", issue.Wires[0].ParamName);
            Assert.Equal("source", issue.Wires[0].PeerComponentName);
            Assert.Contains("does not exist", issue.Wires[0].Reason);
            Assert.Contains("legacyIn", issue.MissingInputNames);
            Assert.Empty(issue.MissingOutputNames);
        }

        [Test]
        public void UpdateComponents_ConnectedOutputRemoved_ReportsMissingOutput()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String legacyOutput = TestDocument.RegisterLegacyOutput(component, "legacyOut");
            Param_String recipient = TestDocument.AddFloatingParam(document, "recipient", 300, 100);

            recipient.AddSource(legacyOutput);
            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.NotNull(updated);
            Assert.Single(issues);

            ManualReconnectionIssue issue = issues[0];
            Assert.Single(issue.Wires);
            Assert.Equal(GH_ParameterSide.Output, issue.Wires[0].Side);
            Assert.Equal("legacyOut", issue.Wires[0].ParamName);
            Assert.Contains("legacyOut", issue.MissingOutputNames);
            Assert.Empty(issue.MissingInputNames);
        }

        [Test]
        public void UpdateComponents_RemovedUnconnectedParameter_NoIssue()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            TestDocument.RegisterLegacyInput(component, "legacyIn");
            TestDocument.RegisterLegacyOutput(component, "legacyOut");
            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.NotNull(updated);
            Assert.Empty(issues);
        }

        [Test]
        public void UpdateComponents_SeveralMissingWires_SingleIssueWithDetails()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String legacyInput = TestDocument.RegisterLegacyInput(component, "legacyIn");
            Param_String legacyOutput = TestDocument.RegisterLegacyOutput(component, "legacyOut");
            Param_String source_1 = TestDocument.AddFloatingParam(document, "source_1", 0, 100);
            Param_String source_2 = TestDocument.AddFloatingParam(document, "source_2", 0, 200);
            Param_String recipient = TestDocument.AddFloatingParam(document, "recipient", 300, 100);

            legacyInput.AddSource(source_1);
            legacyInput.AddSource(source_2);
            recipient.AddSource(legacyOutput);
            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.NotNull(updated);
            Assert.Single(issues);

            ManualReconnectionIssue issue = issues[0];
            Assert.Equal(3, issue.Wires.Count);
            Assert.Equal(2, issue.Wires.Count(x => x.Side == GH_ParameterSide.Input));
            Assert.Equal(1, issue.Wires.Count(x => x.Side == GH_ParameterSide.Output));
            Assert.Single(issue.MissingInputNames);
            Assert.Single(issue.MissingOutputNames);
        }

        [Test]
        public void UpdateComponents_SeveralAffectedComponents_DeterministicOrdering()
        {
            GH_Document document = CreateDocumentOrSkip();

            TestUpdatableComponent component_Bottom = TestDocument.AddComponent<TestUpdatableComponent>(document, 300, 200);
            TestUpdatableComponent component_Top = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String source = TestDocument.AddFloatingParam(document, "source", 0, 100);

            Param_String legacyInput_Bottom = TestDocument.RegisterLegacyInput(component_Bottom, "legacyIn");
            Param_String legacyInput_Top = TestDocument.RegisterLegacyInput(component_Top, "legacyIn");
            legacyInput_Bottom.AddSource(source);
            legacyInput_Top.AddSource(source);

            TestDocument.MakeObsolete(component_Bottom);
            TestDocument.MakeObsolete(component_Top);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component_Bottom, component_Top }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.NotNull(updated);
            Assert.Equal(2, issues.Count);
            Assert.Equal(100, issues[0].PivotY);
            Assert.Equal(200, issues[1].PivotY);
        }

        [Test]
        public void UpdateComponents_FailedReplacement_ReportsOldComponent()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestFailingComponent component = TestDocument.AddComponent<TestFailingComponent>(document, 100, 100);
            TestDocument.MakeObsolete(component);

            TestFailingComponent.ThrowOnConstruct = true;
            try
            {
                List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

                Assert.Null(updated);
                Assert.Single(issues);
                Assert.True(issues[0].ReplacementFailed);
                Assert.Equal(component.InstanceGuid, issues[0].ComponentInstanceGuid);
                Assert.Same(component, document.FindObject(component.InstanceGuid, true));
            }
            finally
            {
                TestFailingComponent.ThrowOnConstruct = false;
            }
        }

        [Test]
        public void UpdateComponents_IssueGuid_ResolvesToReplacement()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String legacyInput = TestDocument.RegisterLegacyInput(component, "legacyIn");
            Param_String source = TestDocument.AddFloatingParam(document, "source", 0, 100);

            legacyInput.AddSource(source);
            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.NotNull(updated);
            Assert.Single(issues);
            Assert.NotEqual(component.InstanceGuid, issues[0].ComponentInstanceGuid);

            IGH_DocumentObject resolved = document.FindObject(issues[0].ComponentInstanceGuid, true);
            Assert.Same(updated[0], resolved);
        }

        [Test]
        public void NavigateTo_StaleGuid_ReturnsFalse()
        {
            GH_Document document = CreateDocumentOrSkip();

            bool navigated = Modify.NavigateTo(document, Guid.NewGuid());

            Assert.False(navigated);
        }

        [Test]
        public void NavigateTo_NoActiveCanvas_ReturnsFalse()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);

            bool navigated = Modify.NavigateTo(document, component.InstanceGuid);

            Assert.False(navigated);
        }

        [Test]
        public void MissingConnections_PeerRemovedFromDocument_ReportsUnavailable()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String source = TestDocument.AddFloatingParam(document, "source", 0, 100);

            component.Params.Input[0].AddSource(source);
            List<ConnectionSnapshot> snapshots = Modify.CaptureConnectionSnapshots(component);
            Assert.Single(snapshots);

            document.RemoveObject(source, false);

            List<ManualReconnectionWire> missing = Query.MissingConnections(component, snapshots);

            Assert.Single(missing);
            Assert.Contains("no longer available", missing[0].Reason);
        }
    }
}
