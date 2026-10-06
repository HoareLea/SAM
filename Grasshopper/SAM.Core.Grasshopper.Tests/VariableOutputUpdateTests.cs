// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;

namespace SAM.Core.Grasshopper.Tests
{
    /// <summary>
    /// <b>Updating a component whose parameters a person inserted themselves.</b>
    /// <para>
    /// A <see cref="GH_SAMVariableOutputParameterComponent"/> registers only its DEFAULT parameters, so the
    /// replacement built by the updater does not carry a voluntary output somebody added and wired up, nor
    /// a voluntary input they typed a value into. The update must bring the replacement to the same
    /// parameter set before the wires go back on it - otherwise every such wire is reported dropped and
    /// every such value is left behind, on exactly the components most recently modernised to declarative
    /// Inputs/Outputs.
    /// </para>
    /// <para>
    /// The fixture is <see cref="TestVariableOutputComponent"/>: inputs a, b, [tolerance]; outputs x,
    /// [successful], y - the voluntary output deliberately in the MIDDLE, so that anything restoring by
    /// position rather than by name hands its wire to the wrong output and is caught here.
    /// </para>
    /// </summary>
    [TestFixture]
    public class VariableOutputUpdateTests
    {
        private const string SkipReason = "GH_Document requires the Rhino native runtime; run these tests in a Rhino-enabled environment.";

        private static GH_Document CreateDocumentOrSkip()
        {
            GH_Document result = TestDocument.TryCreate();
            Skip.If(result == null, SkipReason);
            return result;
        }

        // ---- The starting point ------------------------------------------------------------------------

        /// <summary>A fresh instance carries its defaults and not the voluntary ones - which is what makes this hard.</summary>
        [Test]
        public void FreshComponent_CarriesOnlyItsDefaultParameters()
        {
            CreateDocumentOrSkip();

            TestVariableOutputComponent component = new TestVariableOutputComponent();

            Assert.Equal(new[] { "a", "b" }, Names(component.Params.Input));
            Assert.Equal(new[] { "x", "y" }, Names(component.Params.Output));
        }

        // ---- The failure this fixes --------------------------------------------------------------------

        /// <summary>
        /// A wired voluntary output survives the update, attached to the output it was attached to.
        /// <para>
        /// This is the reported failure. Before the expansion step the replacement had no "successful"
        /// output at all, so the wire was reported dropped and the person was told to reconnect it by hand.
        /// </para>
        /// </summary>
        [Test]
        public void ConnectedVoluntaryOutput_SurvivesTheUpdate()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestVariableOutputComponent component = AddComponent(document);

            IGH_Param param_Successful = Insert(component, GH_ParameterSide.Output, 1);
            Param_String recipient = TestDocument.AddFloatingParam(document, "recipient", 300, 100);
            recipient.AddSource(param_Successful);

            component.SetComponentVersion("0.9.0");

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.Single(updated);
            Assert.Empty(issues);

            GH_SAMComponent component_New = updated[0];

            IGH_Param param_New = Output(component_New, "successful");
            Assert.NotNull(param_New);
            Assert.Contains(param_New, recipient.Sources);
        }

        /// <summary>
        /// The wire lands on the output it came from and not on whichever output happens to sit at its old
        /// index. "successful" is declared between "x" and "y", so a positional restore would attach it to
        /// "y".
        /// </summary>
        [Test]
        public void RestoredOutput_IsTheSameOutputAndNotTheOneAtItsIndex()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestVariableOutputComponent component = AddComponent(document);

            Insert(component, GH_ParameterSide.Output, 1);

            Param_String recipient_Successful = TestDocument.AddFloatingParam(document, "recipient successful", 300, 100);
            Param_String recipient_Y = TestDocument.AddFloatingParam(document, "recipient y", 300, 200);

            recipient_Successful.AddSource(Output(component, "successful"));
            recipient_Y.AddSource(Output(component, "y"));

            component.SetComponentVersion("0.9.0");

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            GH_SAMComponent component_New = Assert.Single(updated);

            Assert.Single(recipient_Successful.Sources);
            Assert.Single(recipient_Y.Sources);
            Assert.Equal("successful", recipient_Successful.Sources[0].Name);
            Assert.Equal("y", recipient_Y.Sources[0].Name);

            //And the recovered output sits where the component declares it, not appended after y.
            Assert.Equal(new[] { "x", "successful", "y" }, Names(component_New.Params.Output));
        }

        /// <summary>A voluntary INPUT keeps both its wire and the value typed into it.</summary>
        [Test]
        public void VoluntaryInput_KeepsItsConnectionAndItsPersistentData()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestVariableOutputComponent component = AddComponent(document);

            IGH_Param param_Tolerance = Insert(component, GH_ParameterSide.Input, 2);
            ((Param_String)param_Tolerance).PersistentData.Append(new GH_String("0.001"));

            Param_String source = TestDocument.AddFloatingParam(document, "source", 0, 200);
            Input(component, "b").AddSource(source);

            component.SetComponentVersion("0.9.0");

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            GH_SAMComponent component_New = Assert.Single(updated);
            Assert.Empty(issues);

            Assert.Equal(new[] { "a", "b", "tolerance" }, Names(component_New.Params.Input));

            Param_String param_New = Input(component_New, "tolerance") as Param_String;
            Assert.NotNull(param_New);
            Assert.Single(param_New.PersistentData.AllData(true));
            Assert.Equal("0.001", param_New.PersistentData.get_FirstItem(true).Value);

            Assert.Contains(source, Input(component_New, "b").Sources);
        }

        /// <summary>Persistent data on a DEFAULT input is not disturbed by the expansion.</summary>
        [Test]
        public void DefaultInput_KeepsItsPersistentData()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestVariableOutputComponent component = AddComponent(document);

            Insert(component, GH_ParameterSide.Input, 2);
            ((Param_String)Input(component, "b")).PersistentData.Append(new GH_String("kept"));

            component.SetComponentVersion("0.9.0");

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Param_String param_New = Input(Assert.Single(updated), "b") as Param_String;

            Assert.NotNull(param_New);
            Assert.Equal("kept", param_New.PersistentData.get_FirstItem(true).Value);
        }

        // ---- What must not change ----------------------------------------------------------------------

        /// <summary>The component keeps its guid, so a saved definition still finds it.</summary>
        [Test]
        public void Update_PreservesTheComponentGuid()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestVariableOutputComponent component = AddComponent(document);

            Insert(component, GH_ParameterSide.Output, 1);
            component.SetComponentVersion("0.9.0");

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            Assert.Equal(component.ComponentGuid, Assert.Single(updated).ComponentGuid);
        }

        /// <summary>
        /// A component that never had the voluntary parameter does not acquire one. The expansion recovers
        /// what was there; it does not decide the component should have more.
        /// </summary>
        [Test]
        public void ComponentWithoutVoluntaryParameters_IsUnchanged()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestVariableOutputComponent component = AddComponent(document);

            component.SetComponentVersion("0.9.0");

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            GH_SAMComponent component_New = Assert.Single(updated);

            Assert.Equal(new[] { "a", "b" }, Names(component_New.Params.Input));
            Assert.Equal(new[] { "x", "y" }, Names(component_New.Params.Output));
            Assert.Empty(issues);
        }

        /// <summary>
        /// A fixed-output component is untouched by any of this: it is not variable, so there is nothing to
        /// expand, and it updates exactly as it did.
        /// </summary>
        [Test]
        public void FixedOutputComponent_StillUpdatesAndReconnects()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestUpdatableComponent component = TestDocument.AddComponent<TestUpdatableComponent>(document, 100, 100);
            Param_String source = TestDocument.AddFloatingParam(document, "source", 0, 100);
            Param_String recipient = TestDocument.AddFloatingParam(document, "recipient", 300, 100);

            component.Params.Input[0].AddSource(source);
            recipient.AddSource(component.Params.Output[0]);
            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            GH_SAMComponent component_New = Assert.Single(updated);

            Assert.Empty(issues);
            Assert.Contains(source, component_New.Params.Input[0].Sources);
            Assert.Contains(component_New.Params.Output[0], recipient.Sources);
        }

        /// <summary>
        /// A parameter the new version genuinely no longer declares is still reported, not fabricated. The
        /// expansion adds only what the declaration knows about.
        /// </summary>
        [Test]
        public void UndeclaredParameter_IsStillReportedAsDropped()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestVariableOutputComponent component = AddComponent(document);

            Param_String param_Legacy = TestDocument.RegisterLegacyOutput(component, "legacyOut");
            Param_String recipient = TestDocument.AddFloatingParam(document, "recipient", 300, 100);
            recipient.AddSource(param_Legacy);

            component.SetComponentVersion("0.9.0");

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            GH_SAMComponent component_New = Assert.Single(updated);

            Assert.Null(Output(component_New, "legacyOut"));
            Assert.Single(issues);
            Assert.Contains("legacyOut", issues[0].MissingOutputNames);
        }

        // ---- Helpers -----------------------------------------------------------------------------------

        private static TestVariableOutputComponent AddComponent(GH_Document gH_Document)
        {
            TestVariableOutputComponent result = new TestVariableOutputComponent();
            result.CreateAttributes();
            result.Attributes.Pivot = new PointF(100, 100);
            gH_Document.AddObject(result, false);

            return result;
        }

        /// <summary>Inserts a voluntary parameter the way the canvas does - through the component's own contract.</summary>
        private static IGH_Param Insert(TestVariableOutputComponent component, GH_ParameterSide side, int index)
        {
            Assert.True(component.CanInsertParameter(side, index));

            IGH_Param result = component.CreateParameter(side, index);
            Assert.NotNull(result);

            if (side == GH_ParameterSide.Input)
            {
                component.Params.RegisterInputParam(result, index);
            }
            else
            {
                component.Params.RegisterOutputParam(result, index);
            }

            component.Params.OnParametersChanged();
            component.VariableParameterMaintenance();

            return result;
        }

        private static IGH_Param Output(IGH_Component component, string name)
        {
            return Find(component.Params.Output, name);
        }

        private static IGH_Param Input(IGH_Component component, string name)
        {
            return Find(component.Params.Input, name);
        }

        private static IGH_Param Find(List<IGH_Param> params_, string name)
        {
            foreach (IGH_Param param in params_)
            {
                if (param != null && param.Name == name)
                {
                    return param;
                }
            }

            return null;
        }

        private static string[] Names(List<IGH_Param> params_)
        {
            List<string> result = new List<string>();
            foreach (IGH_Param param in params_)
            {
                result.Add(param?.Name);
            }

            return result.ToArray();
        }
    }
}
