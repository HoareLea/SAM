// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SAM.Core.Grasshopper.Tests
{
    /// <summary>
    /// <b>An update must not double the values behind a default input that already ships with its own
    /// persistent data.</b>
    /// <para>
    /// A component whose registration bakes in defaults via <c>SetPersistentData(...)</c> - the way
    /// production components such as <c>SAMAnalyticalCreateCaseByApertureByAzimuths</c>'s "_ratios" do -
    /// hands the updater a brand-new replacement parameter that already carries those same defaults.
    /// Copying the old parameter's values onto it by APPENDING rather than REPLACING lands a person's
    /// authored values behind the defaults instead of in place of them.
    /// </para>
    /// </summary>
    [TestFixture]
    public class CopyPersistentDataReplacementTests
    {
        private const string SkipReason = "GH_Document requires the Rhino native runtime; run these tests in a Rhino-enabled environment.";

        private static GH_Document CreateDocumentOrSkip()
        {
            GH_Document result = TestDocument.TryCreate();
            Skip.If(result == null, SkipReason);
            return result;
        }

        [Test]
        public void AuthoredValues_ReplaceTheDefaultsRatherThanAppendToThem()
        {
            GH_Document document = CreateDocumentOrSkip();
            TestDefaultPersistentDataComponent component = new TestDefaultPersistentDataComponent();
            component.CreateAttributes();
            document.AddObject(component, false);

            Param_Number ratiosParam = (Param_Number)Input(component, "ratios");
            Assert.Equal(4, ratiosParam.PersistentData.AllData(true).Count());

            //A person replaced the four defaults with two values of their own.
            ratiosParam.PersistentData.Clear();
            ratiosParam.PersistentData.Append(new GH_Number(0.5));
            ratiosParam.PersistentData.Append(new GH_Number(0.6));
            Assert.Equal(2, ratiosParam.PersistentData.AllData(true).Count());

            TestDocument.MakeObsolete(component);

            List<GH_SAMComponent> updated = Modify.UpdateComponents(new GH_SAMComponent[] { component }, out Log log, out List<ManualReconnectionIssue> issues);

            GH_SAMComponent component_New = Assert.Single(updated);
            Assert.Empty(issues);

            Param_Number ratiosParam_New = Input(component_New, "ratios") as Param_Number;
            Assert.NotNull(ratiosParam_New);

            List<double> values = ratiosParam_New.PersistentData.AllData(true).Select(x => ((GH_Number)x).Value).ToList();

            //The point of the test: exactly the two authored values, not the four defaults plus them.
            Assert.Equal(2, values.Count);
            Assert.Equal(0.5, values[0]);
            Assert.Equal(0.6, values[1]);
        }

        private static IGH_Param Input(IGH_Component component, string name)
        {
            foreach (IGH_Param param in component.Params.Input)
            {
                if (param != null && param.Name == name)
                {
                    return param;
                }
            }

            return null;
        }
    }
}
