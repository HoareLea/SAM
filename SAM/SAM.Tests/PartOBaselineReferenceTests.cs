// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>A saved Part O result says what it was derived from</b> (model-state architecture, PR-5).
    /// <para>
    /// <see cref="PartOBaselineReference"/> is stamped on result models, carries identity (guid, state fingerprint) with a
    /// <b>relative</b> locator and a display name, and is resolved by identity - never by file name, and never through an absolute
    /// path, because none is persisted. Everything here is offline: real <c>.sam</c> files in a temporary folder, no TAS.
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

        /// <summary>The folder a case's result is written to, in the layout a Part O run uses.</summary>
        private string Directory_Result(string name_Case = "Iteration1a") => Path.Combine(directory, "PartO", name_Case, "tas");

        /// <summary>A result: a copy of the design (so it keeps the design's guid) that carries a provenance record, as a run's model does.</summary>
        private static AnalyticalModel Result(AnalyticalModel design, PartOBaselineReference partOBaselineReference)
        {
            AnalyticalModel result = new(design);

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

        private PartOBaselineReference ReferenceTo(AnalyticalModel design, string path_Design, PartODerivedCase partODerivedCase = PartODerivedCase.Iteration1a, string name_Case = "Iteration1a")
        {
            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromDesign(partODerivedCase, design, path_Design, Directory_Result(name_Case));
            Assert.NotNull(partOBaselineReference);

            return partOBaselineReference;
        }

        /// <summary>The whole text a saved <c>.sam</c> holds, inflated.</summary>
        private static string Payload(string path)
        {
            using ZipArchive zipArchive = ZipFile.OpenRead(path);

            StringBuilder stringBuilder = new();
            foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
            {
                using StreamReader streamReader = new(zipArchiveEntry.Open(), Encoding.UTF8);
                stringBuilder.Append(streamReader.ReadToEnd());
            }

            return stringBuilder.ToString();
        }

        // ---- the representation ------------------------------------------------------------------------------

        /// <summary>Every field survives JSON, and a reference of the right shape is valid.</summary>
        [Fact]
        public void JsonRoundTrip_PreservesTheReference()
        {
            AnalyticalModel design = Design();
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, Path.Combine(directory, "Design.sam"), PartODerivedCase.Iteration2, "Iteration2");

            PartOBaselineReference read = new(partOBaselineReference.ToJsonObject());

            Assert.True(read.IsValid);
            Assert.Equal(PartODerivedCase.Iteration2, read.Case);
            Assert.Equal(PartOModelReferenceKind.Design, read.Design.Kind);
            Assert.Equal(design.Guid, read.Design.Guid);
            Assert.Equal(design.Name, read.Design.Name);
            Assert.Equal(SimulationResultProvenance.Fingerprint(design), read.Design.Fingerprint);
            Assert.Equal(Path.Combine("..", "..", "..", "Design.sam"), read.Design.Path_Relative);
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
                Assert.Null(Analytical.Create.PartOBaselineReferenceFromResult(partODerivedCase, result, null, Directory_Result()));
            }

            foreach (PartODerivedCase partODerivedCase in new[] { PartODerivedCase.Iteration2B, PartODerivedCase.Iteration3 })
            {
                Assert.Null(Analytical.Create.PartOBaselineReferenceFromDesign(partODerivedCase, design, null, null));
                Assert.True(Analytical.Create.PartOBaselineReferenceFromResult(partODerivedCase, result, null, Directory_Result()).IsValid);
            }

            Assert.Null(Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.Undefined, design, null, null));
            Assert.Null(Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.Iteration1a, null, null, null));

            //A reference of the wrong shape for its case is invalid even when each part is valid.
            PartOModelReference partOModelReference_Source = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration3, result, null, null).Source;
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration2B, fromDesign.Design, null).IsValid);
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration1a, fromDesign.Design, partOModelReference_Source).IsValid);
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration1a, null, null).IsValid);
            Assert.False(new PartOBaselineReference(PartODerivedCase.Iteration3, new PartOModelReference(PartOModelReferenceKind.Result, design.Guid, "x", "f", null), partOModelReference_Source).IsValid);
        }

        /// <summary>A result with no provenance is not a proven result, so nothing is derived from it.</summary>
        [Fact]
        public void ASourceWithNoProvenance_IsNotASourceResult()
        {
            Assert.Null(Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration2B, Design(), null, null));
        }

        // ---- no absolute path is persisted -------------------------------------------------------------------

        /// <summary>
        /// A saved result can be shared as a fixture or as evidence, so nothing in it may name a workstation, a user or a OneDrive folder. The
        /// reference holds identity and a relative locator only; the absolute path a caller starts from is used to compute the locator and
        /// is not kept.
        /// </summary>
        [Fact]
        public void NoAbsolutePathIsEverPersisted()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            AnalyticalModel result = Result(design, ReferenceTo(design, path_Design));
            string path_Result = Save(result, "PartO", "Iteration1a", "tas", "Design.sam");

            //Neither the reference's JSON nor the saved file mentions the folder they were made in, or any absolute path or drive.
            string json = result.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).ToJsonObject().ToJsonString();
            Assert.DoesNotContain("Path_Absolute", json);
            Assert.DoesNotContain(directory, json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(directory.Replace("\\", "\\\\"), json, StringComparison.OrdinalIgnoreCase);

            string payload = Payload(path_Result);
            Assert.Contains("Path_Relative", payload);
            Assert.DoesNotContain("Path_Absolute", payload);
            Assert.DoesNotContain(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), payload, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(directory.Replace("\\", "\\\\"), payload, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(Environment.UserName, Regex.Match(payload, "\"Design\":\\{[^}]*\\}").Value, StringComparison.OrdinalIgnoreCase);

            PartOBaselineReference read = Open(path_Result).GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);
            Assert.False(Path.IsPathRooted(read.Design.Path_Relative));

            //A reference written by a build that did keep an absolute path is read without it and written back without it.
            System.Text.Json.Nodes.JsonObject jsonObject = read.ToJsonObject();
            ((System.Text.Json.Nodes.JsonObject)jsonObject["Design"])["Path_Absolute"] = path_Design;
            PartOBaselineReference legacy = new(jsonObject);
            Assert.True(legacy.IsValid);
            Assert.DoesNotContain("Path_Absolute", legacy.ToJsonObject().ToJsonString());

            //A design that was never saved, or a result folder not yet known, records no locator at all rather than an absolute one.
            Assert.Null(ReferenceTo(design, null).Design.Path_Relative);
            Assert.Null(Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.Iteration1a, design, path_Design, null).Design.Path_Relative);
        }

        /// <summary>A place the caller knows of now finds a design the relative locator cannot - for that call only, and nothing is recorded.</summary>
        [Fact]
        public void ARuntimeHint_FindsADesignTheLocatorCannot_AndIsNotPersisted()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");
            PartOBaselineReference partOBaselineReference = ReferenceTo(design, path_Design);
            string json_Before = partOBaselineReference.ToJsonObject().ToJsonString();

            //The design moves somewhere the relative locator and its neighbours do not reach.
            string path_Moved = Path.Combine(directory, "elsewhere", "deeper", "Moved.sam");
            Directory.CreateDirectory(Path.GetDirectoryName(path_Moved));
            File.Move(path_Design, path_Moved);

            string path_Result = Path.Combine(Directory_Result(), "Design.sam");

            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineReference.Design.PartOModelResolution(path_Result).Status);

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(path_Result, path_Moved);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineResolution.Status);
            Assert.Equal(Path.GetFullPath(path_Moved), partOBaselineResolution.Path);

            //A hint is held to the same identity rule as any other candidate: another model there is not the design.
            string path_Other = Save(Design("Other"), "other", "Other.sam");
            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineReference.Design.PartOModelResolution(path_Result, path_Other).Status);

            Assert.Equal(json_Before, partOBaselineReference.ToJsonObject().ToJsonString());
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
            string path_Result = Save(result, "PartO", "Iteration1a", "tas", "Design.sam");

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

        /// <summary>2B names the Iteration 2 result it derives from, and still says which design that lineage came from - rebased to its own folder.</summary>
        [Fact]
        public void Iteration2B_ReferencesTheIteration2Result_AndInheritsTheDesign()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            AnalyticalModel result_2 = Result(design, ReferenceTo(design, path_Design, PartODerivedCase.Iteration2, "Iteration2"));
            string path_Result_2 = Save(result_2, "PartO", "Iteration2", "tas", "Design.sam");

            string directory_2B = Directory_Result("Iteration2B");
            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration2B, Open(path_Result_2), path_Result_2, directory_2B);

            Assert.True(partOBaselineReference.IsValid);
            Assert.Equal(PartODerivedCase.Iteration2B, partOBaselineReference.Case);
            Assert.Equal(PartOModelReferenceKind.Result, partOBaselineReference.Source.Kind);
            Assert.Equal(PartOModelReferenceKind.Design, partOBaselineReference.Design.Kind);
            Assert.Equal(design.Guid, partOBaselineReference.Design.Guid);
            Assert.Equal(result_2.GetValue<SimulationResultProvenance>(AnalyticalModelParameter.SimulationResultProvenance).Fingerprint_Model, partOBaselineReference.Source.Fingerprint);

            //Both locators are relative to the folder 2B writes to: the inherited design's was rebased from the Iteration 2 result's folder.
            Assert.Equal(Path.Combine("..", "..", "Iteration2", "tas", "Design.sam"), partOBaselineReference.Source.Path_Relative);
            Assert.Equal(Path.Combine("..", "..", "..", "Design.sam"), partOBaselineReference.Design.Path_Relative);

            //A 2B round copies its Iteration 2 result, so it carries that result's reference until 2B replaces it.
            AnalyticalModel round = new(result_2);
            Assert.Equal(PartODerivedCase.Iteration2, round.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Case);
            Assert.True(round.StampPartOBaselineReference(partOBaselineReference));
            Assert.Equal(PartODerivedCase.Iteration2B, round.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Case);

            string path_Result_2B = Path.Combine(directory_2B, "Design-Opt01.sam");
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Source.PartOModelResolution(path_Result_2B).Status);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Design.PartOModelResolution(path_Result_2B).Status);
        }

        /// <summary>Iteration 3 points at the one 1a/2 result it is paired with, and at the design behind it.</summary>
        [Fact]
        public void Iteration3_ReferencesItsSourceResult_AndTheDesignBehindIt()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            AnalyticalModel result_A = Result(design, ReferenceTo(design, path_Design, PartODerivedCase.Iteration1a));
            string path_Result_A = Save(result_A, "PartO", "Iteration1a", "tas", "Design.sam");

            //A folder of another depth than Reference A's, so the inherited design locator has to be rebased to reach the design.
            string directory_3 = Path.Combine(directory, "PartO", "Iteration3");
            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration3, Open(path_Result_A), path_Result_A, directory_3);
            string path_Result_B = Path.Combine(directory_3, "B.sam");

            Assert.Equal(Path.Combine("..", "..", "Design.sam"), partOBaselineReference.Design.Path_Relative);
            Assert.Equal(Path.Combine("..", "Iteration1a", "tas", "Design.sam"), partOBaselineReference.Source.Path_Relative);

            Assert.True(partOBaselineReference.IsValid);
            Assert.Equal(PartODerivedCase.Iteration3, partOBaselineReference.Case);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Source.PartOModelResolution(path_Result_B).Status);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Design.PartOModelResolution(path_Result_B).Status);
        }

        /// <summary>A result derived from a legacy result (no reference of its own) records its source and says nothing about a design.</summary>
        [Fact]
        public void AResultDerivedFromALegacyResult_RecordsItsSource_AndNoDesign()
        {
            AnalyticalModel legacy = new(Design());
            legacy.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(legacy, null));

            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration3, legacy, null, null);

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
            Assert.False(legacy.LocatePartOBaselineReference(directory, Path.Combine(directory, "Design.sam")));
        }

        // ---- resolution: by identity, never by name ----------------------------------------------------------

        /// <summary>A whole case tree that is copied elsewhere finds the design that was copied with it, through the relative locator.</summary>
        [Fact]
        public void ACopiedTree_FindsItsDesignThroughTheRelativePath()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "project", "Design.sam");
            string directory_Result = Path.Combine(directory, "project", "PartO", "Iteration1a", "tas");

            AnalyticalModel result = Result(design, Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.Iteration1a, design, path_Design, directory_Result));
            Save(result, "project", "PartO", "Iteration1a", "tas", "Design.sam");

            string directory_Copy = Path.Combine(directory, "copy");
            CopyDirectory(Path.Combine(directory, "project"), directory_Copy);
            Directory.Delete(Path.Combine(directory, "project"), true);

            string path_Result_Copy = Path.Combine(directory_Copy, "PartO", "Iteration1a", "tas", "Design.sam");
            PartOBaselineReference read = Open(path_Result_Copy).GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);

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
            string path_Result = Path.Combine(Directory_Result(), "Design.sam");

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
            string path_Result = Path.Combine(Directory_Result(), "Design.sam");
            File.Delete(path_Design);

            //Same name, same content shape, different model (a new guid).
            Save(Design(), "Design.sam");
            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(path_Result);
            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineResolution.Status);
            Assert.Null(partOBaselineResolution.Path);

            //A result keeps its design's guid, but it is a result: never taken for the design.
            AnalyticalModel result = Result(design, partOBaselineReference);
            Save(result, "Design.sam");
            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineReference.Design.PartOModelResolution(path_Result).Status);
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

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(Path.Combine(Directory_Result(), "x.sam"));

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

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(Path.Combine(Directory_Result(), "x.sam"));

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

            PartOBaselineResolution partOBaselineResolution = partOBaselineReference.Design.PartOModelResolution(Path.Combine(Directory_Result(), "x.sam"));

            Assert.Equal(PartOBaselineResolutionStatus.NotFound, partOBaselineResolution.Status);
            Assert.Contains("Design", partOBaselineResolution.Description);
        }

        /// <summary>A source result overwritten by a later run is found by identity but no longer the result 2B/3 derived from.</summary>
        [Fact]
        public void ASourceResultOverwrittenByALaterRun_IsChanged()
        {
            AnalyticalModel design = Design();
            AnalyticalModel result = Result(design, ReferenceTo(design, null, PartODerivedCase.Iteration2, "Iteration2"));
            string path_Result = Save(result, "PartO", "Iteration2", "tas", "Design.sam");
            string path_Result_2B = Path.Combine(Directory_Result("Iteration2B"), "Design-Opt01.sam");
            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration2B, Open(path_Result), path_Result, Directory_Result("Iteration2B"));
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, partOBaselineReference.Source.PartOModelResolution(path_Result_2B).Status);

            AdjacencyCluster adjacencyCluster = result.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Another flat"));
            AnalyticalModel later = new(result, adjacencyCluster);
            later.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(later, null));
            Save(later, "PartO", "Iteration2", "tas", "Design.sam");

            Assert.Equal(PartOBaselineResolutionStatus.Changed, partOBaselineReference.Source.PartOModelResolution(path_Result_2B).Status);
        }

        // ---- the fingerprint invariant -----------------------------------------------------------------------

        /// <summary>
        /// <b>The design fingerprint a reference records is the design's, as handed in, and stamping cannot move it.</b> It is captured from the
        /// intended state; the reference goes on a different model (the result), so the design is untouched; the result's own fingerprint -
        /// which now includes the reference - is a different value that the reference never holds; save and reopen keep it; an unchanged
        /// design resolves, and a genuinely changed one reports Changed.
        /// </summary>
        [Fact]
        public void TheDesignFingerprint_IsCapturedFromTheIntendedState_AndStampingCannotPerturbIt()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            string fingerprint_Intended = SimulationResultProvenance.Fingerprint(Open(path_Design));
            Assert.Equal(fingerprint_Intended, SimulationResultProvenance.Fingerprint(design));

            PartOBaselineReference partOBaselineReference = ReferenceTo(design, path_Design);
            Assert.Equal(fingerprint_Intended, partOBaselineReference.Design.Fingerprint);

            //Stamping goes on the result, and the design is the same model it was, bit for bit.
            AnalyticalModel result = new(design);
            Assert.True(result.StampPartOBaselineReference(partOBaselineReference));
            Assert.Equal(fingerprint_Intended, SimulationResultProvenance.Fingerprint(design));
            Assert.False(design.HasValue(AnalyticalModelParameter.PartOBaselineReference));
            Assert.Equal(fingerprint_Intended, result.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Design.Fingerprint);

            //No recursion: the result's own fingerprint includes the reference, so it is another value, and the reference holds the design's.
            Assert.NotEqual(fingerprint_Intended, SimulationResultProvenance.Fingerprint(result));

            //Stamped a second time, over what it inherited, nothing about the recorded identity moves.
            AnalyticalModel round = new(result);
            Assert.True(round.StampPartOBaselineReference(partOBaselineReference));
            Assert.Equal(fingerprint_Intended, round.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Design.Fingerprint);

            result.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(result, null));
            string path_Result = Save(result, "PartO", "Iteration1a", "tas", "Design.sam");

            PartOBaselineReference read = Open(path_Result).GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);
            Assert.Equal(fingerprint_Intended, read.Design.Fingerprint);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, read.Design.PartOModelResolution(path_Result).Status);

            //A genuine change to the design is reported as one.
            AdjacencyCluster adjacencyCluster = design.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Flat 2"));
            Save(new AnalyticalModel(design, adjacencyCluster), "Design.sam");
            Assert.Equal(PartOBaselineResolutionStatus.Changed, read.Design.PartOModelResolution(path_Result).Status);
        }

        /// <summary>
        /// <b>The source fingerprint a reference records is the source's own recorded state, and stamping a derived model cannot move it.</b>
        /// It equals the source's provenance record and what the source fingerprints to now; the source is another model, so it is untouched;
        /// save and reopen keep it; an unchanged source resolves, and a source whose content changed - even with its old provenance
        /// record still on it - reports Changed.
        /// </summary>
        [Fact]
        public void TheSourceFingerprint_IsCapturedFromTheSourcesRecordedState_AndStampingCannotPerturbIt()
        {
            AnalyticalModel design = Design();
            string path_Design = Save(design, "Design.sam");

            AnalyticalModel source = Result(design, ReferenceTo(design, path_Design, PartODerivedCase.Iteration2, "Iteration2"));
            string path_Source = Save(source, "PartO", "Iteration2", "tas", "Design.sam");

            string fingerprint_Recorded = source.GetValue<SimulationResultProvenance>(AnalyticalModelParameter.SimulationResultProvenance).Fingerprint_Model;
            string fingerprint_Source_Before = SimulationResultProvenance.Fingerprint(source);
            Assert.Equal(fingerprint_Recorded, fingerprint_Source_Before);

            string directory_2B = Directory_Result("Iteration2B");
            PartOBaselineReference partOBaselineReference = Analytical.Create.PartOBaselineReferenceFromResult(PartODerivedCase.Iteration2B, Open(path_Source), path_Source, directory_2B);
            Assert.Equal(fingerprint_Recorded, partOBaselineReference.Source.Fingerprint);

            //Stamping a derived model leaves the source - in memory and on disk - exactly as it was, and the recorded identity unchanged.
            AnalyticalModel round = new(source);
            Assert.True(round.StampPartOBaselineReference(partOBaselineReference));
            Assert.Equal(fingerprint_Source_Before, SimulationResultProvenance.Fingerprint(source));
            Assert.Equal(fingerprint_Recorded, source.GetValue<SimulationResultProvenance>(AnalyticalModelParameter.SimulationResultProvenance).Fingerprint_Model);
            Assert.Equal(fingerprint_Recorded, SimulationResultProvenance.Fingerprint(Open(path_Source)));
            Assert.Equal(fingerprint_Recorded, round.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Source.Fingerprint);

            //No recursion: the derived model's own fingerprint is another value, and the reference holds the source's.
            Assert.NotEqual(fingerprint_Recorded, SimulationResultProvenance.Fingerprint(round));

            round.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(round, null));
            string path_Round = Save(round, "PartO", "Iteration2B", "tas", "Design-Opt01.sam");

            PartOBaselineReference read = Open(path_Round).GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);
            Assert.Equal(fingerprint_Recorded, read.Source.Fingerprint);
            Assert.Equal(PartOBaselineResolutionStatus.Resolved, read.Source.PartOModelResolution(path_Round).Status);
            Assert.Equal(Path.GetFullPath(path_Source), read.Source.PartOModelResolution(path_Round).Path);

            //The source's content changes after it was saved, and it still carries its OLD provenance record: the record is what it said of
            //itself, so it is not trusted - the model is fingerprinted as it is now.
            AnalyticalModel reopened_Source = Open(path_Source);
            AdjacencyCluster adjacencyCluster = reopened_Source.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Edited afterwards"));
            AnalyticalModel edited = new(reopened_Source, adjacencyCluster);
            Assert.Equal(fingerprint_Recorded, edited.GetValue<SimulationResultProvenance>(AnalyticalModelParameter.SimulationResultProvenance).Fingerprint_Model);
            Save(edited, "PartO", "Iteration2", "tas", "Design.sam");

            PartOBaselineResolution partOBaselineResolution = read.Source.PartOModelResolution(path_Round);
            Assert.Equal(PartOBaselineResolutionStatus.Changed, partOBaselineResolution.Status);
            Assert.Equal(Path.GetFullPath(path_Source), partOBaselineResolution.Path);
        }

        // ---- locators ----------------------------------------------------------------------------------------

        /// <summary>Locating gives a design locator where there is none, never replaces one, and never touches identity.</summary>
        [Fact]
        public void Locate_AddsAMissingRelativeLocatorOnly()
        {
            AnalyticalModel design = Design();
            AnalyticalModel result = new(design);
            Assert.True(result.StampPartOBaselineReference(Analytical.Create.PartOBaselineReferenceFromDesign(PartODerivedCase.MixedDesign, design, null, null)));
            Assert.Null(result.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Design.Path_Relative);

            string path_Design = Path.Combine(directory, "Design.sam");

            Assert.True(result.LocatePartOBaselineReference(Directory_Result(), path_Design));

            PartOBaselineReference located = result.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference);
            Assert.Equal(Path.Combine("..", "..", "..", "Design.sam"), located.Design.Path_Relative);
            Assert.Equal(design.Guid, located.Design.Guid);
            Assert.Equal(SimulationResultProvenance.Fingerprint(design), located.Design.Fingerprint);
            Assert.DoesNotContain("Path_Absolute", located.ToJsonObject().ToJsonString());

            //A locator the reference already holds is not replaced by the caller's.
            Assert.False(result.LocatePartOBaselineReference(Directory_Result(), Path.Combine(directory, "Other.sam")));
            Assert.Equal(Path.Combine("..", "..", "..", "Design.sam"), result.GetValue<PartOBaselineReference>(AnalyticalModelParameter.PartOBaselineReference).Design.Path_Relative);
        }

        /// <summary>A locator re-expressed for another folder points at the same file; where any part is unknown there is none.</summary>
        [Fact]
        public void ARebasedLocator_PointsAtTheSameFile()
        {
            string directory_From = Path.Combine(directory, "PartO", "Iteration2", "tas");
            string directory_To = Path.Combine(directory, "PartO", "Iteration3");

            string path_Relative = Analytical.Query.PartOBaselineRelativePath(directory_From, Path.Combine(directory, "Design.sam"));
            Assert.Equal(Path.Combine("..", "..", "..", "Design.sam"), path_Relative);

            string path_Rebased = Analytical.Query.PartOBaselineRebasedPath(path_Relative, directory_From, directory_To);
            Assert.Equal(Path.Combine("..", "..", "Design.sam"), path_Rebased);
            Assert.Equal(Path.GetFullPath(Path.Combine(directory, "Design.sam")), Path.GetFullPath(Path.Combine(directory_To, path_Rebased)));

            Assert.Null(Analytical.Query.PartOBaselineRebasedPath(null, directory_From, directory_To));
            Assert.Null(Analytical.Query.PartOBaselineRebasedPath(path_Relative, null, directory_To));
            Assert.Null(Analytical.Query.PartOBaselineRebasedPath(path_Relative, directory_From, null));
        }

        // ---- a result is never a baseline --------------------------------------------------------------------

        /// <summary>
        /// Carrying the reference marks a model as a result: the validator refuses it as a baseline, Remove Results removes it, and
        /// the design it was derived from is not marked or changed.
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
