// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>PartOBaselineReference</c> on a materialised mixed model (PR-5): the materialisation says which baseline it was built
    /// from, by identity and by the same state fingerprint its record already holds, and the baseline is not touched.
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        [Fact]
        public void Materialised_MixedModel_ReferencesItsBaseline_AndTheBaselineIsUntouched()
        {
            AnalyticalModel baseline = WithStrategies(WithHeldConditions(Baseline()), Mvhr(Flat1), Natural(Flat2), Mvhr(Flat3));
            string json_Baseline = Core.Convert.ToString(baseline);

            PartOMaterialisation partOMaterialisation = Materialise(baseline);

            Assert.True(partOMaterialisation.AnalyticalModel.TryGetValue(AnalyticalModelParameter.PartOBaselineReference, out PartOBaselineReference partOBaselineReference));
            Assert.True(partOBaselineReference.IsValid);
            Assert.Equal(PartODerivedCase.MixedDesign, partOBaselineReference.Case);
            Assert.Null(partOBaselineReference.Source);
            Assert.Equal(PartOModelReferenceKind.Design, partOBaselineReference.Design.Kind);
            Assert.Equal(baseline.Guid, partOBaselineReference.Design.Guid);
            Assert.Equal(baseline.Name, partOBaselineReference.Design.Name);

            //The state is the record's own baseline fingerprint - one hash, two readers - and SAM does not know the baseline's file.
            Assert.Equal(partOMaterialisation.Record.Fingerprint_Baseline, partOBaselineReference.Design.Fingerprint);
            Assert.Equal(SimulationResultProvenance.Fingerprint(baseline), partOBaselineReference.Design.Fingerprint);
            Assert.Null(partOBaselineReference.Design.Path_Relative);

            //The baseline is neither modified nor marked: it stays a clean baseline and carries no reference.
            Assert.Equal(json_Baseline, Core.Convert.ToString(baseline));
            Assert.False(baseline.HasValue(AnalyticalModelParameter.PartOBaselineReference));
            Assert.True(baseline.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            //The record still judges the baseline current: the reference changed nothing it reads.
            Assert.True(partOMaterialisation.Record.IsCurrent(baseline, null, null, out _));
        }

        [Fact]
        public void RemoveRunState_TakesTheBaselineReferenceAway_AndTheCleanedCopyIsABaselineAgain()
        {
            AnalyticalModel baseline = WithHeldConditions(Baseline());
            AnalyticalModel run = PreparedAndRun(baseline);
            Assert.True(run.StampPartOBaselineReference(Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.Iteration1a, baseline, null, null)));

            Assert.Contains(run.PartOBaselineFindings(), x => x.Reason == PartOMaterialisationRefusalReason.RunOutputBaseline && x.Message.Contains("baseline reference"));

            AnalyticalModel cleaned = run.RemovePartORunState(out List<string> removed, out List<string> kept);

            Assert.Contains(removed, x => x.Contains("baseline reference"));
            Assert.Empty(kept);
            Assert.False(cleaned.HasValue(AnalyticalModelParameter.PartOBaselineReference));
            Assert.True(cleaned.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));
        }
    }
}
