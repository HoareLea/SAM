// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>A saved Part O result says what it was derived from</b> (model-state architecture, PR-5).
    /// <para>
    /// <see cref="PartOBaselineReference"/> is stamped on result models, carries identity (guid, state fingerprint) with
    /// locators (relative then absolute path) and a display name, and is resolved by identity - never by file name.
    /// Everything here is offline: real <c>.sam</c> files in a temporary folder, no TAS.
    /// </para>
    /// </summary>
    public class PartOBaselineReferenceTests : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "SAM.PartOBaselineReference." + Guid.NewGuid().ToString("N"));

        public PartOBaselineReferenceTests()
        {
            Directory.CreateDirectory(directory);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch
            {
            }
        }

        // ---- fixtures ----------------------------------------------------------------------------------------

        private static AnalyticalModel Design(string name = "Design", string space = "Flat 1")
        {
            AdjacencyCluster adjacencyCluster = new();
            adjacencyCluster.AddObject(new Space(space));

            return new AnalyticalModel(name, null, null, null, adjacencyCluster, null, null);
        }

        /// <summary>A result: a copy of the design (so it keeps the design's guid) that carries a provenance record, as a run's model does.</summary>
        private static AnalyticalModel Result(AnalyticalModel design, PartOBaselineReference partOBaselineReference, string name = null)
        {
            AnalyticalModel result = new(design);
            if (name is not null)
            {
                result.Name = name;
            }

            Assert.True(result.StampPartOBaselineReference(partOBaselineReference));
            result.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(result, null));

            return result;
        }

        private string Save(AnalyticalModel analyticalModel, params string[] relative)
        {
            string path = Path.Combine([directory, .. relative]);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Assert.True(Core.Convert.ToFile(analyticalModel, path, SAMFileType.SAM));

            return path;
        }

        private static AnalyticalModel Open(string path)
        {
            return Core.Convert.ToSAM<AnalyticalModel>(path).OfType<AnalyticalModel>().Single();
        }

        private PartOBaselineReference ReferenceTo(AnalyticalModel design, string path_Design, PartODerivedCase partODerivedCase = PartODerivedCase.Iteration1a)
        {
            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromDesign(partODerivedCase, design, path_Design);
            Assert.NotNull(partOBaselineReference);

            return partOBaselineReference;
        }

        // ---- the representation ------------------------------------------------------------------------------

        /// <summary>Every field survives JSON, and a reference of the right shape is valid.</summary>
        [Fact]
        public void JsonRoundTrip_PreservesTheReference()
        {
            AnalyticalModel design = Design();
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, Path.Combine(directory, "Design.sam"), PartODerivedCase.Iteration2);
            partOBaselineReference.Design.Path_Relative = Path.Combine("..", "..", "Design.sam");

            PartOBaselineReference read = new(partOBaselineReference.ToJsonObject());

            Assert.True(read.IsValid);
            Assert.Equal(PartODerivedCase.Iteration2, read.Case);
            Assert.Equal(PartOModelReferenceKind.Design, read.Design.Kind);
            Assert.Equal(design.Guid, read.Design.Guid);
            Assert.Equal(design.Name, read.Design.Name);
            Assert.Equal(SimulationResultProvenance.Fingerprint(design), read.Design.Fingerprint);
            Assert.Equal(partOBaselineReference.Design.Path_Absolute, read.Design.Path_Absolute);
            Assert.Equal(partOBaselineReference.Design.Path_Relative, read.Design.Path_Relative);
            Assert.Null(read.Source);
            Assert.Equal(partOBaselineReference.ToJsonObject().ToJsonString(), read.ToJsonObject().ToJsonString());
        }

        /// <summary>A schema this build does not know is not applied, and is written back as it was read.</summary>
        [Fact]
        public void AnUnknownSchema_LoadsInvalid_AndRoundTripsAsRead()
        {
            PartOBaselineReference partOBaselineReference = ReferenceTo(Design(), null);
            System.Text.Json.Nodes.JsonObject jsonObject = partOBaselineReference.ToJsonObject();
            jsonObject["Schema"] = "PartOBaselineReference:v99";

            PartOBaselineReference read = new(jsonObject);

            Assert.False(read.IsValid);
            Assert.Equal("PartOBaselineReference:v99", read.SchemaRead);
            Assert.Equal("PartOBaselineReference:v99", (string)read.ToJsonObject()["Schema"]);
        }

        /// <summary>The right reference for each case: a design for 1a/1b/2/Mixed, a source result for 2B/3, never both shapes.</summary>
        [Fact]
        public void EachCaseNeedsItsOwnKindOfReference()
        {
            AnalyticalModel design = Design();
            PartOBaselineReference fromDesign = ReferenceTo(design, null);
            AnalyticalModel result = Result(design, fromDesign);

            foreach (PartODerivedCase partODerivedCase in new[] { PartODerivedCase.Iteration1a, PartODerivedCase.Iteration1b, PartODerivedCase.Iteration2, PartODerivedCase.MixedDesign })
            {
                Assert.True(ReferenceTo(design, null, partODerivedCase).IsValid);
                Assert.Null(Analytical.Create.PartOBaselineReferenceFromResult(partODerivedCase, result, null));
            }

            foreach (PartODerivedCase partODerivedCase in new[] { PartODerivedCase.Iteration2B, PartODerivedCase.Iteration3 })
            {
                Assert.Null(Analytical.Create.PartOBaselineReferenceFromDesign(partODerivedCase, design, null));
                Assert.True(Analytical.Create.PartOBaselineReferenceFromResult(partODerivedCase, result, null).IsValid);
            }

            Assert.Null(Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.Undefined, design, null));
            Assert.Null(Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.Iteration1a, null, null));

            //A reference of the wrong shape for its case is invalid even when each part is valid.
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration2B, fromDesign.Design, null).IsValid);
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration1a, fromDesign.Design, Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration3, result, null).Source).IsValid);
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration1a, null, null).IsValid);
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration3, new PartOModelReference(PartOModelReferenceKind.Result, design.Guid, "x", "f", null), Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration3, result, null).Source).IsValid);
        }

        /// <summary>A result with no provenance is not a proven result, so nothing is derived from it.</summary>
        [Fact]
        public void ASourceWithNoProvenance_IsNotASourceResult()
        {
            Assert.Null(Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration2B, Design(), null));
        }

        // ---- 1a / 1b / 2 / 2B / 3 ----------------------------------------------------------------------------

        /// <summary>1a, 1b and 2 results reference the design they were derived from - by identity and state.</summary>
        [Theory]
        [InlineData(PartODerivedCase.Iteration1a)]
        [InlineData(PartODerivedCase.Iteration1b)]
        [InlineData(PartODerivedCase.Iteration2)]
        public void ADesignDerivedResult_ReferencesItsDesign_AfterSaveAndReopen(PartODerivedCase partODerivedCase)
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            AnalyticalModel result = Result(design, ReferenceTo(design, path_Design, partODerivedCase));
            string path_Result = Save(result, "PartO", "Case", "tas", "Design.sam");

            AnalyticalModel reopened = Open(path_Result);

            Assert.True(reopened.TryGetValue(AnalyticalModelParameter.PartOBaselineReference, out PartOBaselineReference read));
            Assert.True(read.IsValid);
            Assert.Equal(partODerivedCase, read.Case);
            Assert.Null(read.Source);
            Assert.Equal(design.Guid, read.Design.Guid);
            Assert.Equal(SimulationResultProvenance.Fingerprint(design), read.Design.Fingerprint);

            //The saved result still matches its own provenance: the reference was stamped before it was taken.
            Assert.Equal(reopened.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance simulationResultProvenance) ? simulationResultProvenance.Fingerprint_Model : null, SimulationResultProvenance.Fingerprint(reopened));

            PartOBaselineResolution partOBaselineResolution = read.Design.PartOModelResolution(path_Result);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineResolution.Status);
            Assert.Equal(Path.GetFullPath(path_Design), partOBaselineResolution.Path);
        }

        /// <summary>2B names the Iteration 2 result it derives from, and still says which design that lineage came from.</summary>
        [Fact]
        public void Iteration2B_ReferencesTheIteration2Result_AndInheritsTheDesign()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            AnalyticalModel result_2 = Result(design, ReferenceTo(design, path_Design, PartODerivedCase.Iteration2));
            string path_Result_2 = Save(result_2, "PartO", "Iteration2", "tas", "Design.sam");

            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration2B, Open(path_Result_2), path_Result_2);

            Assert.True(partOBaselineReference.IsValid);
            Assert.Equal(PartODerivedCase.Iteration2B, partOBaselineReference.Case);
            Assert.Equal(PartOModelReferenceKind.Result, partOBaselineReference.Source.Kind);
            Assert.Equal(PartOModelReferenceKind.Design, partOBaselineReference.Design.Kind);
            Assert.Equal(design.Guid, partOBaselineReference.Design.Guid);
            Assert.Equal(Path.GetFullPath(path_Design), Path.GetFullPath(partOBaselineReference.Design.Path_Absolute));
            Assert.Equal(result_2.GetValue<SimulationResultProvenance>(AnalyticalModelParameter.SimulationResultProvenance).Fingerprint_Model, partOBaselineReference.Source.Fingerprint);

            //A 2B round copies its Iteration 2 result, so it carries that result's reference until 2B replaces it.
            AnalyticalModel round = new(result_2);
            Assert.Equal(PartODerivedCase.Iteration2, round.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Case);
            Assert.True(round.StampPartOBaselineReference(partOBaselineReference));
            Assert.Equal(PartODerivedCase.Iteration2B, round.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Case);

            Assert.True(round.LocatePartOBaselineReference(Path.GetDirectoryName(Path.Combine(directory, "PartO", "Iteration2B", "tas", "x.sam"))));
            PartOBaselineReference located = round.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);
            Assert.Equal(Path.Combine("..", "..", "Iteration2", "tas", "Design.sam").Replace(Path.DirectorySeparatorChar, '/'), located.Source.Path_Relative.Replace(Path.DirectorySeparatorChar, '/'));
            Assert.Equal(Path.Combine("..", "..", "..", "Design.sam").Replace(Path.DirectorySeparatorChar, '/'), located.Design.Path_Relative.Replace(Path.DirectorySeparatorChar, '/'));
        }

        /// <summary>Iteration 3 points at the 1a/2 result it is paired with, and at the design behind it.</summary>
        [Fact]
        public void Iteration3_ReferencesItsSourceResult_AndTheDesignBehindIt()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            AnalyticalModel result_A = Result(design, ReferenceTo(design, path_Design, PartODerivedCase.Iteration1a));
            string path_Result_A = Save(result_A, "PartO", "Iteration1a", "tas", "Design.sam");

            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration3, Open(path_Result_A), path_Result_A);

            Assert.True(partOBaselineReference.IsValid);
            Assert.Equal(PartODerivedCase.Iteration3, partOBaselineReference.Case);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Source.PartOModelResolution(Path.Combine(directory, "PartO", "Iteration3", "tas", "B.sam")).Status);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Design.PartOModelResolution(Path.Combine(directory, "PartO", "Iteration3", "tas", "B.sam")).Status);
        }

        /// <summary>A result derived from a legacy result (no reference of its own) records its source and says nothing about a design.</summary>
        [Fact]
        public void AResultDerivedFromALegacyResult_RecordsItsSource_AndNoDesign()
        {
            AnalyticalModel legacy = new(Design());
            legacy.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(legacy, null));

            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration3, legacy, null);

            Assert.True(partOBaselineReference.IsValid);
            Assert.Null(partOBaselineReference.Design);
            Assert.NotNull(partOBaselineReference.Source);
        }

        // ---- legacy and absent -------------------------------------------------------------------------------

        /// <summary>A result saved before this existed has no reference; nothing is inferred, and no reference resolves to anything.</summary>
        [Fact]
        public void AModelWithNoReference_IsUnknown_NotGuessed()
        {
            AnalyticalModel legacy = new(Design());
            legacy.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(legacy, null));
            AnalyticalModel reopened = Open(Save(legacy, "legacy.sam"));

            Assert.False(reopened.HasValue(AnalyticalModelParameter.PartOBaselineReference));
            Assert.False(reopened.TryGetValue(AnalyticalModelParameter.PartOBaselineReference, out PartOBaselineReference _));

            Assert.Equal(PartOBaselineResolutionStatus.Unknown, ((PartOModelReference)null).PartOModelResolution(null).Status);
            Assert.Equal(PartOBaselineResolutionStatus.Unknown, new PartOModelReference().PartOModelResolution(null).Status);

            Assert.False(legacy.StampPartOBaselineReference(new PartOBaselineReference()));
            Assert.False(legacy.LocatePartOBaselineReference(directory));
        }

        // ---- resolution: by identity, never by name ----------------------------------------------------------

        /// <summary>A whole case tree that is copied elsewhere finds the design that was copied with it, through the relative locator.</summary>
        [Fact]
        public void ACopiedTree_FindsItsDesignThroughTheRelativePath()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "project", "Design.sam");
            AnalyticalModel result = Result(design, ReferenceTo(design, path_Design));
            string path_Result = Path.Combine(directory, "project", "PartO", "Iteration1a", "tas", "Design.sam");
            Assert.True(result.LocatePartOBaselineReference(Path.GetDirectoryName(path_Result)));
            result.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(result, null));
            Save(result, "project", "PartO", "Iteration1a", "tas", "Design.sam");

            string directory_Copy = Path.Combine(directory, "copy");
            CopyDirectory(Path.Combine(directory, "project"), directory_Copy);
            Directory.Delete(Path.Combine(directory, "project"), true);

            string path_Result_Copy = Path.Combine(directory_Copy, "PartO", "Iteration1a", "tas", "Design.sam");
            PartOBaselineReference read = Open(path_Result_Copy).GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);

            Assert.False(File.Exists(read.Design.Path_Absolute));
            PartOBaselineResolution partOBaselineResolution = read.Design.PartOModelResolution(path_Result_Copy);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineResolution.Status);
            Assert.Equal(Path.GetFullPath(Path.Combine(directory_Copy, "Design.sam")), partOBaselineResolution.Path);
        }

        /// <summary>A design that was renamed is still found beside where it was: by guid and state, not by name.</summary>
        [Fact]
        public void ARenamedDesign_IsFoundByIdentity()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, path_Design);
            string path_Result = Path.Combine(directory, "PartO", "Case", "tas", "Design.sam");

            string path_Renamed = Path.Combine(directory, "Renamed to something else.sam");
            File.Move(path_Design, path_Renamed);

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(path_Result);

            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineResolution.Status);
            Assert.Equal(path_Renamed, partOBaselineResolution.Path);
        }

        /// <summary>A file with the right name and another identity is not the design, and neither is a result that shares the design's guid.</summary>
        [Fact]
        public void NameAloneNeverMakesAModelTheDesign()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, path_Design);
            File.Delete(path_Design);

            //Same name, same content shape, different model (a new guid).
            Save(Design(), "Design.sam");
            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(null);
            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineResolution.Status);
            Assert.Null(partOBaselineResolution.Path);

            //A result keeps its design's guid, but it is a result: never taken for the design.
            AnalyticalModel result = Result(design, partOBaselineReference);
            Save(result, "Design.sam");
            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineReference.Design.PartOModelResolution(null).Status);
        }

        /// <summary>The design moved on since the result was derived: found, and said to have changed.</summary>
        [Fact]
        public void ADesignEditedSinceTheRun_IsFoundButChanged()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, path_Design);

            AnalyticalModel edited = new(design, new AdjacencyCluster(design.AdjacencyCluster));
            AdjacencyCluster adjacencyCluster = edited.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Flat 2"));
            Save(new AnalyticalModel(edited, adjacencyCluster), "Design.sam");

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(Path.Combine(directory, "PartO", "Case", "tas", "x.sam"));

            Assert.Equal(PartOBaselineResolutionStatus.Changed, partOBaselineResolution.Status);
            Assert.Equal(path_Design, partOBaselineResolution.Path);
            Assert.False(partOBaselineResolution.IsResolved);
        }

        /// <summary>Two files beside the recorded place that are both the model in its recorded state: none is chosen.</summary>
        [Fact]
        public void TwoIdenticalCandidates_AreAmbiguous_AndNoneIsChosen()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, path_Design);
            File.Delete(path_Design);

            Save(design, "Copy one.sam");
            Save(design, "Copy two.sam");

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(null);

            Assert.Equal(PartOBaselineResolutionStatus.Ambiguous, partOBaselineResolution.Status);
            Assert.Null(partOBaselineResolution.Path);
        }

        /// <summary>The design is gone: said plainly, with no path.</summary>
        [Fact]
        public void AMissingDesign_IsNotFound()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, path_Design);
            File.Delete(path_Design);

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(null);

            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineResolution.Status);
            Assert.Contains("Design", partOBaselineResolution.Description);
        }

        /// <summary>A source result overwritten by a later run is found by identity but no longer the result 2B/3 derived from.</summary>
        [Fact]
        public void ASourceResultOverwrittenByALaterRun_IsChanged()
        {
            AnalyticalModel design = Design();
            AnalyticalModel result = Result(design, ReferenceTo(design, null, PartODerivedCase.Iteration2));
            string path_Result = Save(result, "PartO", "Iteration2", "tas", "Design.sam");
            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration2B, Open(path_Result), path_Result);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Source.PartOModelResolution(null).Status);

            AdjacencyCluster adjacencyCluster = result.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Another flat"));
            AnalyticalModel later = new(result, adjacencyCluster);
            later.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(later, null));
            Save(later, "PartO", "Iteration2", "tas", "Design.sam");

            Assert.Equal(PartOBaselineResolutionStatus.Changed, partOBaselineReference.Source.PartOModelResolution(null).Status);
        }

        // ---- locators ----------------------------------------------------------------------------------------

        /// <summary>Locating completes the relative path and the design file, and never touches identity.</summary>
        [Fact]
        public void Locate_AddsLocatorsOnly()
        {
            AnalyticalModel design = Design();
            AnalyticalModel result = new(design);
            Assert.True(result.StampPartOBaselineReference(ReferenceTo(design, null)));

            string directory_Result = Path.Combine(directory, "PartO", "Iteration1a", "tas");
            string path_Design = Path.Combine(directory, "Design.sam");

            Assert.True(result.LocatePartOBaselineReference(directory_Result, path_Design));

            PartOBaselineReference located = result.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);
            Assert.Equal(path_Design, located.Design.Path_Absolute);
            Assert.Equal(Path.Combine("..", "..", "..", "Design.sam"), located.Design.Path_Relative);
            Assert.Equal(design.Guid, located.Design.Guid);
            Assert.Equal(SimulationResultProvenance.Fingerprint(design), located.Design.Fingerprint);

            //A path the reference already holds is not replaced by the caller's.
            Assert.True(result.LocatePartOBaselineReference(directory_Result, Path.Combine(directory, "Other.sam")));
            Assert.Equal(path_Design, result.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Design.Path_Absolute);
        }

        // ---- a result is never a baseline --------------------------------------------------------------------

        /// <summary>
        /// Carrying the reference marks a model as a result: the validator refuses it as a baseline, Remove Results removes it, and
        /// opening the result resolves its design without making either one the other.
        /// </summary>
        [Fact]
        public void AResultWithTheReference_IsRefusedAsABaseline_AndDoesNotBecomeTheDesign()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");
            byte[] bytes_Design = File.ReadAllBytes(path_Design);

            //A model that carries only the reference (no scenarios, no provenance) is still a result.
            AnalyticalModel referenceOnly = new(design);
            Assert.True(referenceOnly.StampPartOBaselineReference(ReferenceTo(design, path_Design)));
            Assert.Contains(referenceOnly.PartOBaselineFindings(), x => x.Reason == PartOMaterialisationRefusalReason.RunOutputBaseline && x.Message.Contains("baseline reference"));

            //The design, before and after a result was derived from it, is untouched and is still a clean baseline.
            Assert.False(design.HasValue(AnalyticalModelParameter.PartOBaselineReference));
            Assert.True(design.IsPartOCleanBaseline(out _));
            Assert.Equal(bytes_Design, File.ReadAllBytes(path_Design));

            //Remove Results takes the reference away, and what it referenced is not touched.
            AnalyticalModel cleaned = referenceOnly.RemovePartORunState(out System.Collections.Generic.List<string> removed, out _);
            Assert.False(cleaned.HasValue(AnalyticalModelParameter.PartOBaselineReference));
            Assert.Contains(removed, x => x.Contains("baseline reference"));
            Assert.True(cleaned.IsPartOCleanBaseline(out _));
        }

        private static void CopyDirectory(string from, string to)
        {
            Directory.CreateDirectory(to);
            foreach (string file in Directory.GetFiles(from))
            {
                File.Copy(file, Path.Combine(to, Path.GetFileName(file)));
            }

            foreach (string directory in Directory.GetDirectories(from))
            {
                CopyDirectory(directory, Path.Combine(to, Path.GetFileName(directory)));
            }
        }
    }
}
