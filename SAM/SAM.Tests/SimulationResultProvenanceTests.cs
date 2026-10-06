// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>SimulationResultProvenance</c> - the persisted record that lets a reopened model be paired with
    /// the results it was produced from, and refuses the pairing when either side no longer matches.
    /// </summary>
    public class SimulationResultProvenanceTests
    {
        private static string WriteResults(string path)
        {
            File.WriteAllText(path, string.Format("results - {0}", Guid.NewGuid()));

            return path;
        }

        /// <summary>
        /// One model, with as much or as little of its non-cluster simulation state as a test needs. The
        /// cluster is always the same, so a test that varies only the location, a library or a parameter is
        /// isolating exactly that input.
        /// </summary>
        private static AnalyticalModel Model(string name, Location location = null, MaterialLibrary materialLibrary = null, ProfileLibrary profileLibrary = null)
        {
            AdjacencyCluster adjacencyCluster = new();
            adjacencyCluster.AddObject(new Space("Bedroom 1"));

            return new AnalyticalModel(name, null, location, null, adjacencyCluster, materialLibrary, profileLibrary);
        }

        /// <summary>The record round-trips through JSON with every field intact.</summary>
        [Fact]
        public void JsonRoundTrip_PreservesTheRecord()
        {
            string path_TSD = WriteResults(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tsd"));

            try
            {
                SimulationResultProvenance provenance = new(Model("run"), path_TSD);

                SimulationResultProvenance read = new(provenance.ToJsonObject());

                //The absolute path is runtime only: what travels is the relative locator.
                Assert.Null(read.Path_TSD);
                Assert.Equal(Path.GetFileName(path_TSD), read.Locator_TSD);
                Assert.Equal(provenance.Length_TSD, read.Length_TSD);
                Assert.Equal(provenance.Timestamp_TSD, read.Timestamp_TSD);
                Assert.Equal(provenance.Fingerprint_Model, read.Fingerprint_Model);
                Assert.Equal(provenance.Fingerprint_OverheatingScenarios, read.Fingerprint_OverheatingScenarios);

                //Every required field is there, so the record can state the rule rather than part of it.
                Assert.True(read.IsComplete);
            }
            finally
            {
                File.Delete(path_TSD);
            }
        }

        /// <summary>No absolute or local path is emitted by the serialization.</summary>
        [Fact]
        public void ToJsonObject_NeverEmitsAnAbsoluteOrLocalPath()
        {
            string directory = NewDirectory();

            try
            {
                string path_TSD = WriteResults(Path.Combine(directory, "run.tsd"));
                SimulationResultProvenance provenance = new(Model("run"), path_TSD, Path.Combine(directory, "run.sam"));

                string json = provenance.ToJsonObject().ToJsonString();

                Assert.DoesNotContain("Path_TSD", json);
                Assert.DoesNotContain(directory.Replace("\\", "\\\\"), json, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain(Environment.UserName, json, StringComparison.OrdinalIgnoreCase);
                Assert.False(Path.IsPathRooted(provenance.Locator_TSD));
                Assert.Equal("run.tsd", provenance.Locator_TSD);

                //A results file in a folder below and above the model is each a relative locator; none at all has none.
                Assert.Equal("results/run.tsd", SimulationResultProvenance.Locator(Path.Combine(directory, "results", "run.tsd"), Path.Combine(directory, "run.sam")));
                Assert.Equal("../run.tsd", SimulationResultProvenance.Locator(Path.Combine(directory, "run.tsd"), Path.Combine(directory, "m", "run.sam")));
                Assert.Null(SimulationResultProvenance.Locator(null, null));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>A model saved with the relative locator and reopened resolves its results, with no path hint and no absolute path in the file.</summary>
        [Fact]
        public void SaveAndReopen_ResolvesTheResultsByTheRelativeLocator()
        {
            string directory = NewDirectory();

            try
            {
                string path_TSD = WriteResults(Path.Combine(directory, "run.tsd"));
                string path_Model = Path.Combine(directory, "run.sam");

                AnalyticalModel analyticalModel = Model("run");
                analyticalModel.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(analyticalModel, path_TSD));

                AnalyticalModel analyticalModel_Reopened = SaveAndReopen(analyticalModel, path_Model);

                string text = File.ReadAllText(path_Model);
                Assert.DoesNotContain("Path_TSD", text);
                Assert.DoesNotContain(directory.Replace("\\", "\\\\"), text, StringComparison.OrdinalIgnoreCase);

                Assert.True(analyticalModel_Reopened.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance provenance));
                Assert.Null(provenance.Path_TSD);
                Assert.True(provenance.IsComplete);
                Assert.True(provenance.TryResolvePath_TSD(analyticalModel_Reopened, path_Model, out string path_Resolved, out string refusal), refusal);
                Assert.Equal(path_TSD, path_Resolved);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>The model and its results copied to another folder still resolve - to the copy, since the locator is relative.</summary>
        [Fact]
        public void CopiedFolder_StillResolvesThroughTheRelativeLocator()
        {
            string directory = NewDirectory();

            try
            {
                string directory_Run = Directory.CreateDirectory(Path.Combine(directory, "run")).FullName;
                string path_TSD = WriteResults(Path.Combine(directory_Run, "run.tsd"));
                string path_Model = Path.Combine(directory_Run, "run.sam");

                AnalyticalModel analyticalModel = Model("run");
                analyticalModel.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(analyticalModel, path_TSD, path_Model));
                SaveAndReopen(analyticalModel, path_Model);

                //Copied with its write time: the same results file in a new place.
                string directory_Copy = Directory.CreateDirectory(Path.Combine(directory, "copy")).FullName;
                string path_TSD_Copy = Path.Combine(directory_Copy, "run.tsd");
                string path_Model_Copy = Path.Combine(directory_Copy, "run.sam");
                File.Copy(path_TSD, path_TSD_Copy);
                File.SetLastWriteTimeUtc(path_TSD_Copy, File.GetLastWriteTimeUtc(path_TSD));
                File.Copy(path_Model, path_Model_Copy);

                //The original is gone, as on another machine.
                File.Delete(path_TSD);
                File.Delete(path_Model);

                AnalyticalModel analyticalModel_Copy = Core.Convert.ToSAM<AnalyticalModel>(path_Model_Copy).OfType<AnalyticalModel>().Single();
                Assert.True(analyticalModel_Copy.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance provenance));
                Assert.True(provenance.TryResolvePath_TSD(analyticalModel_Copy, path_Model_Copy, out string path_Resolved, out string refusal), refusal);
                Assert.Equal(path_TSD_Copy, path_Resolved);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// A model saved by an earlier build carries only the absolute <c>Path_TSD</c>. It still reads, still resolves
        /// (at the recorded path, and beside the model when the folder moved), and written again it drops the path.
        /// </summary>
        [Fact]
        public void LegacyAbsolutePath_StillReadsResolvesAndIsDroppedOnTheNextSave()
        {
            string directory = NewDirectory();

            try
            {
                string path_TSD = WriteResults(Path.Combine(directory, "run.tsd"));
                string path_Model = Path.Combine(directory, "run.sam");

                AnalyticalModel analyticalModel = Model("run");
                SimulationResultProvenance current = new(analyticalModel, path_TSD);

                //What the earlier build wrote: the record's JSON with the absolute path and without the locator.
                System.Text.Json.Nodes.JsonObject legacy = current.ToJsonObject();
                legacy.Remove("Locator_TSD");
                legacy["Path_TSD"] = path_TSD;

                SimulationResultProvenance read = new(legacy);
                Assert.Equal(path_TSD, read.Path_TSD);
                Assert.Equal("run.tsd", read.Locator_TSD);
                Assert.True(read.IsComplete);
                Assert.True(read.TryResolvePath_TSD(analyticalModel, null, out string path_Resolved, out string refusal), refusal);
                Assert.Equal(path_TSD, path_Resolved);

                //The recorded path is stale (another machine) but the file sits beside the model: found by the derived locator.
                legacy["Path_TSD"] = Path.Combine(directory, "gone", "run.tsd");
                SimulationResultProvenance read_Moved = new(legacy);
                Assert.True(read_Moved.TryResolvePath_TSD(analyticalModel, path_Model, out string path_Moved, out string refusal_Moved), refusal_Moved);
                Assert.Equal(path_TSD, path_Moved);

                //Saved again: the path is gone from the output.
                Assert.DoesNotContain("Path_TSD", read.ToJsonObject().ToJsonString());
                Assert.Equal("run.tsd", read.ToJsonObject()["Locator_TSD"]?.GetValue<string>());
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>Missing results refuse exactly as before - safe, named, and without running the model half.</summary>
        [Fact]
        public void MissingResults_RefuseWithTheSameSafeMessage_ForALocatorOnlyRecord()
        {
            string directory = NewDirectory();

            try
            {
                string path_TSD = WriteResults(Path.Combine(directory, "run.tsd"));
                string path_Model = Path.Combine(directory, "run.sam");

                AnalyticalModel analyticalModel = Model("run");
                SimulationResultProvenance saved = new(new SimulationResultProvenance(analyticalModel, path_TSD).ToJsonObject());
                Assert.Null(saved.Path_TSD);

                File.Delete(path_TSD);

                Assert.False(saved.TryResolvePath_TSD(analyticalModel, path_Model, out string path_Resolved, out string refusal));
                Assert.Null(path_Resolved);
                Assert.Contains("are no longer at", refusal);
                Assert.Contains(path_TSD, refusal, StringComparison.OrdinalIgnoreCase);

                //Rewritten rather than missing is still told apart.
                WriteResults(path_TSD);
                File.SetLastWriteTimeUtc(path_TSD, DateTime.UtcNow.AddMinutes(5));
                Assert.False(saved.TryResolvePath_TSD(analyticalModel, path_Model, out string _, out string refusal_Rewritten));
                Assert.Contains("have been rewritten", refusal_Rewritten);

                //No locator and no path at all: the record states no results file.
                Assert.False(new SimulationResultProvenance().TryResolvePath_TSD(analyticalModel, path_Model, out string _, out string refusal_None));
                Assert.Contains("records no simulation results file", refusal_None);

                //A rooted locator is never followed.
                SimulationResultProvenance rooted = new() { Locator_TSD = path_TSD, Length_TSD = 1, Timestamp_TSD = 1, Fingerprint_Model = "a", Fingerprint_OverheatingScenarios = "b" };
                Assert.False(rooted.TryResolvePath_TSD(analyticalModel, path_Model, out string _, out string _));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>Current means: exists, same length, same write time. Anything less is not these results.</summary>
        [Fact]
        public void IsCurrent_RequiresTheExactRecordedFile()
        {
            string path_TSD = WriteResults(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tsd"));

            try
            {
                SimulationResultProvenance provenance = new(Model("run"), path_TSD);

                Assert.True(provenance.IsCurrent(path_TSD));
                Assert.False(provenance.IsCurrent(path_TSD + ".other.tsd"));

                File.AppendAllText(path_TSD, "changed");

                Assert.False(provenance.IsCurrent(path_TSD));
            }
            finally
            {
                File.Delete(path_TSD);
            }
        }

        /// <summary>
        /// The model half: the same model hashes to the same fingerprint after a JSON round trip - which is
        /// the whole reopen path - and an edit changes it.
        /// </summary>
        [Fact]
        public void IsCurrent_ModelFingerprint()
        {
            AnalyticalModel analyticalModel = Model("run");

            string fingerprint = SimulationResultProvenance.Fingerprint(analyticalModel);

            //Stable across a save and a reload - the identities survive serialization, and so does the hash.
            AnalyticalModel analyticalModel_Reopened = new(analyticalModel.ToJsonObject());
            Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_Reopened));

            SimulationResultProvenance provenance = new(analyticalModel, null);

            Assert.True(provenance.IsCurrent(analyticalModel_Reopened));

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Kitchen"));
            Assert.False(provenance.IsCurrent(new AnalyticalModel(analyticalModel, adjacencyCluster)));
        }

        /// <summary>
        /// <b>A record missing either fingerprint validates nothing.</b> This replaces the rule that let an
        /// absent model fingerprint pass on the results side alone. That permissiveness had no legacy to
        /// serve - a model saved before this record existed carries no record at all, and is handled as a
        /// model that was never simulated - and it made a half-written record look like a whole one.
        /// </summary>
        [Fact]
        public void IsCurrent_FailsClosedOnAMissingFingerprint()
        {
            AnalyticalModel analyticalModel = Model("run");

            SimulationResultProvenance provenance = new(analyticalModel, null);

            //The unmodified record still validates the model it was taken from.
            Assert.True(provenance.IsCurrent(analyticalModel));
            Assert.False(provenance.IsComplete);       //no results file was recorded above

            //Neither half may be absent.
            SimulationResultProvenance provenance_NoModel = new(provenance) { Fingerprint_Model = string.Empty };
            Assert.False(provenance_NoModel.IsCurrent(analyticalModel));

            SimulationResultProvenance provenance_NoScenarios = new(provenance) { Fingerprint_OverheatingScenarios = string.Empty };
            Assert.False(provenance_NoScenarios.IsCurrent(analyticalModel));
        }

        /// <summary>
        /// <b>The scenario half, which the cluster fingerprint cannot see.</b> The overheating scenarios are
        /// model-level state, so a model whose scenarios were swapped after its run has an UNCHANGED cluster
        /// fingerprint - and <c>PartORun.Restore</c> reads those scenarios back as the authority the results
        /// are reassessed under. Pinned here at the fingerprint: same scenarios digest the same across a save
        /// and a reload, a different set does not, and a reordering of the same set is not a difference.
        /// </summary>
        [Fact]
        public void Fingerprint_Scenarios_BindsTheAssessmentContext()
        {
            OverheatingScenario overheatingScenario_A = new(PartOAssessmentScope.Dwelling, Guid.NewGuid(), PartOIteration.BasePassive);
            OverheatingScenario overheatingScenario_B = new(PartOAssessmentScope.Dwelling, Guid.NewGuid(), PartOIteration.AcousticRestricted);

            AnalyticalModel analyticalModel = Model("run");
            analyticalModel.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([overheatingScenario_A, overheatingScenario_B]));

            string fingerprint = SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel);

            //Survives the save and the reload, which is the whole reopen path.
            Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint_Scenarios(new AnalyticalModel(analyticalModel.ToJsonObject())));

            //The order a collection happens to hold a set of assessments in is not part of what they state.
            AnalyticalModel analyticalModel_Reordered = Model("run");
            analyticalModel_Reordered.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([overheatingScenario_B, overheatingScenario_A]));
            Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel_Reordered));

            //A different assessment authority is a different fingerprint - a changed iteration...
            AnalyticalModel analyticalModel_Changed = Model("run");
            analyticalModel_Changed.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([overheatingScenario_A, new OverheatingScenario(PartOAssessmentScope.Dwelling, overheatingScenario_B.ZoneGuid, PartOIteration.BasePassive)]));
            Assert.NotEqual(fingerprint, SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel_Changed));

            //...and a dropped scenario, which is what makes the count part of the digest.
            AnalyticalModel analyticalModel_Subset = Model("run");
            analyticalModel_Subset.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([overheatingScenario_A]));
            Assert.NotEqual(fingerprint, SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel_Subset));

            //A model carrying none is not a model carrying some.
            Assert.NotEqual(fingerprint, SimulationResultProvenance.Fingerprint_Scenarios(Model("run")));
            Assert.Equal(16, SimulationResultProvenance.Fingerprint_Scenarios(null).Length);
        }

        /// <summary>
        /// <b>The fingerprint is exactly the digest of the sectioned simulation-bearing state, and is
        /// computed without materializing any of it.</b>
        /// <para>
        /// The definition is what the record documents and what a later session recomputes, so it has to stay
        /// byte-for-byte what hashing the same sections' <c>ToJsonString()</c> produces - this pins that
        /// against a future drift in writer options or section order, either of which would silently
        /// invalidate every recorded digest. The streaming exists because a real ~5,000-space project's
        /// cluster JSON is hundreds of megabytes, and holding it as a string and a byte array to produce
        /// sixteen characters is a multi-gigabyte spike.
        /// </para>
        /// </summary>
        [Fact]
        public void Fingerprint_IsTheDigestOfTheSectionedSimulationState()
        {
            AnalyticalModel analyticalModel = Model("run");

            ulong expected = 14695981039346656037;

            void Digest(byte[] bytes)
            {
                foreach (byte value in bytes)
                {
                    expected ^= value;
                    expected *= 1099511628211;
                }
            }

            //Section 1 is the cluster. Sections 2-5 - material library, profile library, location and model
            //parameters - are absent on this model, so each contributes its tag and nothing else, which is
            //what distinguishes an absent component from an empty one.
            Digest([1]);
            Digest(Encoding.UTF8.GetBytes(analyticalModel.AdjacencyCluster.ToJsonObject().ToJsonString()));
            Digest([2]);
            Digest([3]);
            Digest([4]);
            Digest([5]);

            Assert.Equal(expected.ToString("x16", System.Globalization.CultureInfo.InvariantCulture), SimulationResultProvenance.Fingerprint(analyticalModel));

            //A model with nothing to hash still answers, rather than throwing on the way to a null cluster.
            Assert.Equal(16, SimulationResultProvenance.Fingerprint(null).Length);
        }

        /// <summary>
        /// <b>The fingerprint covers every simulation input, not just the adjacency cluster.</b>
        /// <para>
        /// An <c>AnalyticalModel</c> keeps its simulation inputs in several places. A digest over the cluster
        /// alone did not move when a user edited a material's conductivity, an occupancy profile, the site
        /// location or a model-level parameter after a run - so <c>TryResolvePath_TSD</c> accepted, and a
        /// review presented, results produced from a design that no longer existed. Each of these is the same
        /// cluster with one non-cluster input changed, so each one isolates a hole the old definition had.
        /// </para>
        /// </summary>
        [Fact]
        public void Fingerprint_MovesWhenAnyNonClusterSimulationInputChanges()
        {
            string fingerprint = SimulationResultProvenance.Fingerprint(Model("run"));

            //A material's thermal properties - the conductivity a construction is simulated with.
            MaterialLibrary materialLibrary = new("Library");
            materialLibrary.Add(Analytical.Create.OpaqueMaterial("Brick", "Masonry", "Brick", null, 0.77, 1000, 1700, 0.1, 1, 0.5, 0.5, 0.5, 0.5, 0.9, 0.9, false));

            MaterialLibrary materialLibrary_Changed = new("Library");
            materialLibrary_Changed.Add(Analytical.Create.OpaqueMaterial("Brick", "Masonry", "Brick", null, 0.99, 1000, 1700, 0.1, 1, 0.5, 0.5, 0.5, 0.5, 0.9, 0.9, false));

            string fingerprint_Material = SimulationResultProvenance.Fingerprint(Model("run", materialLibrary: materialLibrary));

            Assert.NotEqual(fingerprint, fingerprint_Material);
            Assert.NotEqual(fingerprint_Material, SimulationResultProvenance.Fingerprint(Model("run", materialLibrary: materialLibrary_Changed)));

            //An occupancy profile - the hourly pattern a space is simulated under.
            ProfileLibrary profileLibrary = new("Library");
            profileLibrary.Add(new Profile("Occupancy", "Occupancy", [0.0, 0.5, 1.0]));

            ProfileLibrary profileLibrary_Changed = new("Library");
            profileLibrary_Changed.Add(new Profile("Occupancy", "Occupancy", [0.0, 0.9, 1.0]));

            string fingerprint_Profile = SimulationResultProvenance.Fingerprint(Model("run", profileLibrary: profileLibrary));

            Assert.NotEqual(fingerprint, fingerprint_Profile);
            Assert.NotEqual(fingerprint_Profile, SimulationResultProvenance.Fingerprint(Model("run", profileLibrary: profileLibrary_Changed)));

            //The site - which decides where the sun is.
            string fingerprint_Location = SimulationResultProvenance.Fingerprint(Model("run", location: new Location("London", -0.13, 51.5, 11)));

            Assert.NotEqual(fingerprint, fingerprint_Location);
            Assert.NotEqual(fingerprint_Location, SimulationResultProvenance.Fingerprint(Model("run", location: new Location("Manchester", -2.24, 53.48, 38))));

            //And a model-level parameter - here the north angle, which rotates every solar gain in the model.
            AnalyticalModel analyticalModel_North = Model("run");
            analyticalModel_North.SetValue(AnalyticalModelParameter.NorthAngle, 0.0);

            AnalyticalModel analyticalModel_North_Changed = Model("run");
            analyticalModel_North_Changed.SetValue(AnalyticalModelParameter.NorthAngle, 1.5);

            Assert.NotEqual(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_North));
            Assert.NotEqual(SimulationResultProvenance.Fingerprint(analyticalModel_North), SimulationResultProvenance.Fingerprint(analyticalModel_North_Changed));
        }

        /// <summary>
        /// <b>The two parameters the model fingerprint must not see.</b>
        /// <para>
        /// <b>This record itself</b> is a structural exclusion: the digest is stored inside it, so a
        /// fingerprint that saw it would change the moment it was stamped and could never agree with itself.
        /// Pinned by stamping a record and recomputing - the value has to be the one the record already
        /// holds.
        /// </para>
        /// <para>
        /// <b>The overheating scenarios</b> are excluded because they have their own authoritative digest,
        /// taken over scenario identity rather than serialized form so that a rename is not a difference.
        /// Digesting them here as well would bind their presentation fields through the back door. So a
        /// change of scenarios moves the scenario fingerprint and leaves the model fingerprint alone - and
        /// <c>IsCurrent</c> still refuses, because it checks both.
        /// </para>
        /// </summary>
        [Fact]
        public void Fingerprint_ExcludesTheProvenanceRecordAndTheOverheatingScenarios()
        {
            string path_TSD = WriteResults(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tsd"));

            try
            {
                AnalyticalModel analyticalModel = Model("run");

                string fingerprint = SimulationResultProvenance.Fingerprint(analyticalModel);

                //The SAME model, copied before anything is stamped on it - not a second Model(), whose
                //cluster would hold a differently guid'd space and digest differently for that reason alone.
                AnalyticalModel analyticalModel_Unstamped = new(analyticalModel);

                //Stamping the record does not move the digest the record states - which is what makes the
                //record able to validate itself.
                SimulationResultProvenance provenance = new(analyticalModel, path_TSD);

                analyticalModel.SetValue(AnalyticalModelParameter.SimulationResultProvenance, provenance);

                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel));
                Assert.Equal(provenance.Fingerprint_Model, SimulationResultProvenance.Fingerprint(analyticalModel));

                //A model carrying nothing but a provenance record digests as one carrying no parameters at
                //all, so the empty parameter set the removal leaves behind is not a difference.
                Assert.Equal(SimulationResultProvenance.Fingerprint(analyticalModel_Unstamped), SimulationResultProvenance.Fingerprint(analyticalModel));

                //Relabelling a case does not invalidate a simulation. CaseDescription is a free-form label
                //the Grasshopper case components write and CaseDataCollection is Design Explorer study
                //metadata; digesting either would refuse valid results and force a fresh annual run because
                //somebody renamed a study - the same false refusal the model's own name is excluded to avoid.
                AnalyticalModel analyticalModel_Relabelled = new(analyticalModel_Unstamped);

                analyticalModel_Relabelled.SetValue("CaseDescription", "South facing, 40% glazing");
                analyticalModel_Relabelled.SetValue(AnalyticalModelParameter.CaseDataCollection, new CaseDataCollection());

                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_Relabelled));

                //The scenarios move their OWN fingerprint and not the model's...
                AnalyticalModel analyticalModel_Scenarios = new(analyticalModel);

                analyticalModel_Scenarios.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([
                    new OverheatingScenario(PartOAssessmentScope.Dwelling, Guid.NewGuid(), PartOIteration.BasePassive)]));

                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_Scenarios));
                Assert.NotEqual(SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel), SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel_Scenarios));

                //...and IsCurrent still refuses, because it is the conjunction of the two.
                Assert.True(provenance.IsCurrent(analyticalModel));
                Assert.False(provenance.IsCurrent(analyticalModel_Scenarios));
            }
            finally
            {
                File.Delete(path_TSD);
            }
        }

        /// <summary>
        /// <b>The digest survives the round trip a saved model actually makes.</b> The fingerprint is
        /// recorded in one session and recomputed in another from a model read back off disk, so a
        /// definition that depended on anything the serializer does not preserve - dictionary ordering,
        /// parameter-set ordering - would refuse every perfectly valid saved run. Every section is populated
        /// here, so the round trip is exercised over all of them rather than the cluster alone.
        /// </summary>
        [Fact]
        public void Fingerprint_SurvivesTheModelsOwnJsonRoundTrip()
        {
            MaterialLibrary materialLibrary = new("Library");
            materialLibrary.Add(Analytical.Create.OpaqueMaterial("Brick", "Masonry", "Brick", null, 0.77, 1000, 1700, 0.1, 1, 0.5, 0.5, 0.5, 0.5, 0.9, 0.9, false));

            ProfileLibrary profileLibrary = new("Library");
            profileLibrary.Add(new Profile("Occupancy", "Occupancy", [0.0, 0.5, 1.0]));

            AnalyticalModel analyticalModel = Model("run", location: new Location("London", -0.13, 51.5, 11), materialLibrary: materialLibrary, profileLibrary: profileLibrary);

            analyticalModel.SetValue(AnalyticalModelParameter.NorthAngle, 1.5);
            analyticalModel.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([
                new OverheatingScenario(PartOAssessmentScope.Dwelling, Guid.NewGuid(), PartOIteration.BasePassive)]));

            AnalyticalModel analyticalModel_Read = new(analyticalModel.ToJsonObject());

            Assert.Equal(SimulationResultProvenance.Fingerprint(analyticalModel), SimulationResultProvenance.Fingerprint(analyticalModel_Read));
            Assert.Equal(SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel), SimulationResultProvenance.Fingerprint_Scenarios(analyticalModel_Read));
        }

        /// <summary>
        /// <b>Absent results are refused on the results side, before the model is digested.</b> The file
        /// checks are stats; the model check hashes the whole project. This pins the order through the
        /// refusal it produces: a record whose file is gone AND whose model has since changed reports the
        /// missing file - the half that was reached - not the model.
        /// </summary>
        [Fact]
        public void TryResolvePath_TSD_RefusesAbsentResultsWithoutTheModelHalf()
        {
            string path_TSD = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tsd");

            AnalyticalModel analyticalModel = Model("run");

            WriteResults(path_TSD);
            SimulationResultProvenance provenance = new(analyticalModel, path_TSD);
            File.Delete(path_TSD);

            //Both halves now fail: the file is gone, and the model is not the one recorded.
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            adjacencyCluster.AddObject(new Space("Kitchen"));

            Assert.False(provenance.TryResolvePath_TSD(new AnalyticalModel(analyticalModel, adjacencyCluster), null, out string path_Resolved, out string refusal));

            Assert.Null(path_Resolved);
            Assert.Contains("no longer at", refusal);
            Assert.DoesNotContain("The model has changed", refusal);
        }

        /// <summary>
        /// And the model half still refuses where the results file is intact - the check moved order, not
        /// away.
        /// </summary>
        [Fact]
        public void TryResolvePath_TSD_StillRefusesAChangedModel()
        {
            string path_TSD = WriteResults(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tsd"));

            try
            {
                AnalyticalModel analyticalModel = Model("run");

                SimulationResultProvenance provenance = new(analyticalModel, path_TSD);

                Assert.True(provenance.TryResolvePath_TSD(analyticalModel, null, out string path_Resolved, out string refusal));
                Assert.Equal(path_TSD, path_Resolved);
                Assert.Null(refusal);

                AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
                adjacencyCluster.AddObject(new Space("Kitchen"));

                Assert.False(provenance.TryResolvePath_TSD(new AnalyticalModel(analyticalModel, adjacencyCluster), null, out string _, out string refusal_Changed));
                Assert.Contains("The model has changed", refusal_Changed);
            }
            finally
            {
                File.Delete(path_TSD);
            }
        }

        /// <summary>
        /// <b>The case the model fingerprint alone cannot see.</b> The cluster is untouched and the results
        /// file is byte-for-byte the recorded one; only the overheating scenarios were swapped after the run.
        /// Before the scenario fingerprint existed, that validated - and the old results would then have been
        /// reassessed under an authority that did not produce them. It is now refused, and the refusal names
        /// the scenarios rather than blaming the model or the file.
        /// </summary>
        [Fact]
        public void TryResolvePath_TSD_RefusesScenariosChangedUnderUnchangedResults()
        {
            string path_TSD = WriteResults(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tsd"));

            try
            {
                Guid guid_Zone = Guid.NewGuid();

                AnalyticalModel analyticalModel = Model("run");
                analyticalModel.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Zone, PartOIteration.BasePassive)]));

                SimulationResultProvenance provenance = new(analyticalModel, path_TSD);

                Assert.True(provenance.TryResolvePath_TSD(analyticalModel, null, out string path_Resolved, out string refusal));
                Assert.Equal(path_TSD, path_Resolved);
                Assert.Null(refusal);

                //Scenario A becomes Scenario B on the SAME model file: same cluster, same results, different
                //assessment authority. This is the exact sequence the record now binds.
                analyticalModel.SetValue(AnalyticalModelParameter.OverheatingScenarios, new SAMCollection<OverheatingScenario>([new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Zone, PartOIteration.AcousticRestricted)]));

                //The unchanged halves are still unchanged - so the refusal below is the scenario half alone.
                Assert.Equal(provenance.Fingerprint_Model, SimulationResultProvenance.Fingerprint(analyticalModel));
                Assert.True(provenance.IsCurrent(path_TSD));

                Assert.False(provenance.TryResolvePath_TSD(analyticalModel, null, out string _, out string refusal_Scenarios));
                Assert.Contains("overheating scenarios", refusal_Scenarios);
                Assert.DoesNotContain("The model has changed", refusal_Scenarios);
            }
            finally
            {
                File.Delete(path_TSD);
            }
        }

        /// <summary>
        /// <b>An incomplete record resolves nothing, and says which field is missing.</b> Every required
        /// field is checked before the filesystem is touched, so a half-written record is refused without
        /// being partially believed - and without hashing a large project on the way to refusing it.
        /// </summary>
        [Fact]
        public void TryResolvePath_TSD_RefusesAnIncompleteRecord()
        {
            string path_TSD = WriteResults(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tsd"));

            try
            {
                AnalyticalModel analyticalModel = Model("run");

                SimulationResultProvenance provenance = new(analyticalModel, path_TSD);

                Assert.True(provenance.IsComplete);
                Assert.True(provenance.TryResolvePath_TSD(analyticalModel, null, out string _, out string _));

                //No model fingerprint.
                SimulationResultProvenance provenance_NoModel = new(provenance) { Fingerprint_Model = string.Empty };
                Assert.False(provenance_NoModel.IsComplete);
                Assert.False(provenance_NoModel.TryResolvePath_TSD(analyticalModel, null, out string path_NoModel, out string refusal_NoModel));
                Assert.Null(path_NoModel);
                Assert.Contains("does not state the design", refusal_NoModel);

                //No scenario fingerprint.
                SimulationResultProvenance provenance_NoScenarios = new(provenance) { Fingerprint_OverheatingScenarios = string.Empty };
                Assert.False(provenance_NoScenarios.IsComplete);
                Assert.False(provenance_NoScenarios.TryResolvePath_TSD(analyticalModel, null, out string path_NoScenarios, out string refusal_NoScenarios));
                Assert.Null(path_NoScenarios);
                Assert.Contains("does not state the overheating scenarios", refusal_NoScenarios);

                //No recorded size or write time - a file check that could never be satisfied is refused as
                //what it is, rather than left to fail as a missing file.
                SimulationResultProvenance provenance_NoFileStats = new(provenance) { Length_TSD = -1, Timestamp_TSD = -1 };
                Assert.False(provenance_NoFileStats.IsComplete);
                Assert.False(provenance_NoFileStats.IsCurrent(path_TSD));
                Assert.False(provenance_NoFileStats.TryResolvePath_TSD(analyticalModel, null, out string path_NoFileStats, out string refusal_NoFileStats));
                Assert.Null(path_NoFileStats);
                Assert.Contains("size and write time", refusal_NoFileStats);
            }
            finally
            {
                File.Delete(path_TSD);
            }
        }

        private static string NewDirectory()
        {
            string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);

            return directory;
        }

        /// <summary>
        /// A ground temperature as a TAS weather year states it: twelve monthly values and NaN - not stated -
        /// for the soil properties TAS weather does not carry. <c>SAM.Weather.Tas</c> builds exactly this for
        /// every model simulated on TAS weather. <paramref name="conductivity"/> is varied by the tests that
        /// state one.
        /// </summary>
        private static Weather.GroundTemperature GroundTemperature(double conductivity, double temperature)
        {
            return new Weather.GroundTemperature(double.NaN, conductivity, double.NaN, double.NaN,
                temperature, temperature, temperature, temperature, temperature, temperature,
                temperature, temperature, temperature, temperature, temperature, temperature);
        }

        /// <summary>A copy of <paramref name="analyticalModel"/> carrying weather with the one ground temperature given.</summary>
        private static AnalyticalModel WithWeather(AnalyticalModel analyticalModel, Weather.GroundTemperature groundTemperature)
        {
            Weather.WeatherData weatherData = new("Leeds_TRY", "CIBSE Weather 2021", 53.836, -1.55, 50);
            weatherData.SetValue(Weather.WeatherDataParameter.GroundTemperatures, new SAMCollection<Weather.GroundTemperature>([groundTemperature]));

            AnalyticalModel result = new(analyticalModel);
            result.SetValue(AnalyticalModelParameter.WeatherData, weatherData);

            return result;
        }

        /// <summary>The path a real run model takes: SAM's native <c>.sam</c> writer, then the ordinary Open.</summary>
        private static AnalyticalModel SaveAndReopen(AnalyticalModel analyticalModel, string path_Model)
        {
            Assert.True(Core.Convert.ToFile(analyticalModel, path_Model, SAMFileType.SAM));

            return Core.Convert.ToSAM<AnalyticalModel>(path_Model).OfType<AnalyticalModel>().Single();
        }

        /// <summary>
        /// <b>Through a real <c>.sam</c> file, not only an in-memory JSON round trip.</b> Every section is
        /// populated, the file is written by the writer Save As uses and read back by the ordinary Open, and a
        /// second save and reopen changes nothing further.
        /// </summary>
        [Fact]
        public void Fingerprint_SurvivesASavedAndReopenedSamFile()
        {
            string directory = NewDirectory();

            try
            {
                MaterialLibrary materialLibrary = new("Library");
                materialLibrary.Add(Analytical.Create.OpaqueMaterial("Brick", "Masonry", "Brick", null, 0.77, 1000, 1700, 0.1, 1, 0.5, 0.5, 0.5, 0.5, 0.9, 0.9, false));

                ProfileLibrary profileLibrary = new("Library");
                profileLibrary.Add(new Profile("Occupancy", "Occupancy", [0.0, 0.5, 1.0]));

                AnalyticalModel analyticalModel = Model("run", location: new Location("London", -0.13, 51.5, 11), materialLibrary: materialLibrary, profileLibrary: profileLibrary);
                analyticalModel.SetValue(AnalyticalModelParameter.NorthAngle, 1.5);

                string fingerprint = SimulationResultProvenance.Fingerprint(analyticalModel);

                AnalyticalModel analyticalModel_Reopened = SaveAndReopen(analyticalModel, Path.Combine(directory, "run.sam"));
                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_Reopened));

                AnalyticalModel analyticalModel_ReopenedAgain = SaveAndReopen(analyticalModel_Reopened, Path.Combine(directory, "run-again.sam"));
                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_ReopenedAgain));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// <b>The defect Part O review reopened into.</b> A model simulated on TAS weather carries ground
        /// temperatures whose soil properties are not stated (NaN). <c>GroundTemperature</c> omits a NaN
        /// property from its JSON - and used to read the omission back as <c>0</c>, so every save and reopen
        /// turned "not stated" into a stated zero. That is a real change to the model's weather, so the
        /// fingerprint, correctly, moved with it: the record stamped before the save refused the model read
        /// back from it ("The model has changed ..."), on every TAS-weather run, with nothing edited at all.
        /// <para>
        /// Pinned end to end: fingerprint, stamp, save, reopen - the reopened model digests to the recorded
        /// value, does so again after a second save, and its own record resolves its results.
        /// </para>
        /// </summary>
        [Fact]
        public void Fingerprint_SurvivesASavedAndReopenedSamFile_WithUnstatedGroundProperties()
        {
            string directory = NewDirectory();

            try
            {
                string path_TSD = WriteResults(Path.Combine(directory, "run.tsd"));
                string path_Model = Path.Combine(directory, "run.sam");

                AnalyticalModel analyticalModel = WithWeather(Model("run"), GroundTemperature(double.NaN, 8.5));

                string fingerprint = SimulationResultProvenance.Fingerprint(analyticalModel);

                analyticalModel.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(analyticalModel, path_TSD));

                AnalyticalModel analyticalModel_Reopened = SaveAndReopen(analyticalModel, path_Model);

                //Not stated stays not stated - which is the whole fix.
                Assert.True(analyticalModel_Reopened.TryGetValue(AnalyticalModelParameter.WeatherData, out Weather.WeatherData weatherData));
                Assert.True(weatherData.TryGetValue(Weather.WeatherDataParameter.GroundTemperatures, out SAMCollection<Weather.GroundTemperature> groundTemperatures));
                Weather.GroundTemperature groundTemperature = Assert.Single(groundTemperatures!);
                Assert.True(double.IsNaN(groundTemperature.Depth));
                Assert.True(double.IsNaN(groundTemperature.Conductivity));
                Assert.True(double.IsNaN(groundTemperature.Density));
                Assert.True(double.IsNaN(groundTemperature.SpecificHeat));
                Assert.Equal(8.5, groundTemperature[11]);

                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_Reopened));

                //Repeatable: a second save and reopen digests to the same value again.
                AnalyticalModel analyticalModel_ReopenedAgain = SaveAndReopen(analyticalModel_Reopened, Path.Combine(directory, "run-again.sam"));
                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_ReopenedAgain));

                //And the record that travelled with the model resolves its results - the review path itself.
                Assert.True(analyticalModel_Reopened.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance provenance));
                Assert.True(provenance.TryResolvePath_TSD(analyticalModel_Reopened, path_Model, out string path_Resolved, out string refusal), refusal);
                Assert.Equal(path_TSD, path_Resolved);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// <b>Stable is not the same as blind.</b> The weather is a simulation input and stays in the digest:
        /// a different monthly ground temperature, a soil property newly stated, and "not stated" against a
        /// stated zero are each a different model. And a model edited AFTER it was reopened is still refused -
        /// the reopen no longer moves the fingerprint, an edit still does.
        /// </summary>
        [Fact]
        public void Fingerprint_StillMovesWhenTheWeatherIsGenuinelyChanged()
        {
            string directory = NewDirectory();

            try
            {
                AnalyticalModel analyticalModel = Model("run");

                string fingerprint = SimulationResultProvenance.Fingerprint(WithWeather(analyticalModel, GroundTemperature(double.NaN, 8.5)));

                Assert.NotEqual(fingerprint, SimulationResultProvenance.Fingerprint(WithWeather(analyticalModel, GroundTemperature(double.NaN, 9.5))));
                Assert.NotEqual(fingerprint, SimulationResultProvenance.Fingerprint(WithWeather(analyticalModel, GroundTemperature(1.5, 8.5))));

                //Not stated is not zero - the fix preserves the distinction rather than collapsing it.
                Assert.NotEqual(fingerprint, SimulationResultProvenance.Fingerprint(WithWeather(analyticalModel, GroundTemperature(0.0, 8.5))));

                //Stamped, saved, reopened - valid - then genuinely edited: refused, and blamed on the model.
                string path_TSD = WriteResults(Path.Combine(directory, "run.tsd"));
                string path_Model = Path.Combine(directory, "run.sam");

                AnalyticalModel analyticalModel_Run = WithWeather(analyticalModel, GroundTemperature(double.NaN, 8.5));
                analyticalModel_Run.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(analyticalModel_Run, path_TSD));

                AnalyticalModel analyticalModel_Reopened = SaveAndReopen(analyticalModel_Run, path_Model);
                Assert.True(analyticalModel_Reopened.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance provenance));
                Assert.True(provenance.TryResolvePath_TSD(analyticalModel_Reopened, path_Model, out string _, out string refusal), refusal);

                AnalyticalModel analyticalModel_Edited = WithWeather(analyticalModel_Reopened, GroundTemperature(double.NaN, 9.5));

                Assert.False(provenance.TryResolvePath_TSD(analyticalModel_Edited, path_Model, out string path_Edited, out string refusal_Edited));
                Assert.Null(path_Edited);
                Assert.Contains("The model has changed", refusal_Edited);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// A copy of <paramref name="analyticalModel"/> carrying the application's view state in the shape SAM_UI
        /// writes it: a parameter set of its own, <c>SAM.Analytical.UI</c>, holding the one parameter
        /// <c>UI Geometry Settings</c>, whose legend text and colours SAM_UI regenerates every time a model is
        /// opened. Built through JSON because the type is SAM_UI's and this assembly cannot see it - which is
        /// also exactly what a model written by SAM_UI and read by SAM alone looks like.
        /// </summary>
        private static AnalyticalModel WithViewSettings(AnalyticalModel analyticalModel, string legendText, int red)
        {
            System.Text.Json.Nodes.JsonObject jsonObject_ViewSettings = new()
            {
                ["_type"] = "SAM.Geometry.UI.UIGeometrySettings,SAM.Geometry.UI",
                ["ViewSettings"] = new System.Text.Json.Nodes.JsonArray(new System.Text.Json.Nodes.JsonObject
                {
                    ["Name"] = "Level 0 [0.1m] IC",
                    ["Legend"] = new System.Text.Json.Nodes.JsonObject
                    {
                        ["LegendItems"] = new System.Text.Json.Nodes.JsonArray(new System.Text.Json.Nodes.JsonObject
                        {
                            ["Text"] = legendText,
                            ["Color"] = new System.Text.Json.Nodes.JsonObject { ["Red"] = red, ["Green"] = 183, ["Blue"] = 178 },
                        }),
                    },
                }),
            };

            ParameterSet parameterSet = new("SAM.Analytical.UI");
            parameterSet.Add("UI Geometry Settings", jsonObject_ViewSettings);

            System.Text.Json.Nodes.JsonObject jsonObject = analyticalModel.ToJsonObject()!;
            System.Text.Json.Nodes.JsonArray jsonArray_ParameterSets = jsonObject["ParameterSets"] as System.Text.Json.Nodes.JsonArray ?? [];
            jsonObject["ParameterSets"] = jsonArray_ParameterSets;

            //Replaced, not added beside - which is what regenerating the view state on open does.
            foreach (System.Text.Json.Nodes.JsonNode? jsonNode in jsonArray_ParameterSets.Where(x => x?["Name"]?.GetValue<string>() == "SAM.Analytical.UI").ToList())
            {
                jsonArray_ParameterSets.Remove(jsonNode);
            }

            jsonArray_ParameterSets.Add(parameterSet.ToJsonObject());

            return new AnalyticalModel(jsonObject);
        }

        /// <summary>
        /// <b>The application's view state is not a simulation input.</b> SAM_UI keeps its views on the model and
        /// regenerates their legend on every open - measured on the licensed Part O acceptance model, 29 legend
        /// colours and labels changed between the stamped model and the one <c>PartORun.Restore</c> fingerprints,
        /// and nothing else did - so a digest that saw them refused every reopened run, correctly by its own
        /// definition and wrongly by its purpose. Views sit beside the model's name and case labels: presentation,
        /// excluded by name. Pinned: views or none digest alike; a model stamped with views, saved, reopened and
        /// given regenerated views still resolves its results; and a genuine design change made beside the views
        /// is still refused.
        /// </summary>
        [Fact]
        public void Fingerprint_IgnoresTheApplicationsViewSettings()
        {
            string directory = NewDirectory();

            try
            {
                AnalyticalModel analyticalModel = WithWeather(Model("run"), GroundTemperature(double.NaN, 8.5));

                string fingerprint = SimulationResultProvenance.Fingerprint(analyticalModel);

                AnalyticalModel analyticalModel_Viewed = WithViewSettings(analyticalModel, "Studio - Studio 1_0 - Studio 1_0 - Studio 1_0", 118);
                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_Viewed));
                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(WithViewSettings(analyticalModel, "Studio - Studio 1_0", 156)));

                string path_TSD = WriteResults(Path.Combine(directory, "run.tsd"));
                string path_Model = Path.Combine(directory, "run.sam");

                analyticalModel_Viewed.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(analyticalModel_Viewed, path_TSD));

                AnalyticalModel analyticalModel_Reopened = SaveAndReopen(analyticalModel_Viewed, path_Model);

                //What the application does to it on open, before it asks for the results.
                AnalyticalModel analyticalModel_Regenerated = WithViewSettings(analyticalModel_Reopened, "Studio - Studio 1_0", 156);

                Assert.True(analyticalModel_Regenerated.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance provenance));
                Assert.True(provenance.TryResolvePath_TSD(analyticalModel_Regenerated, path_Model, out string path_Resolved, out string refusal), refusal);
                Assert.Equal(path_TSD, path_Resolved);

                //Views are ignored; the design beside them is not.
                AnalyticalModel analyticalModel_Edited = new(analyticalModel_Regenerated);
                analyticalModel_Edited.SetValue(AnalyticalModelParameter.NorthAngle, 1.5);

                Assert.False(provenance.TryResolvePath_TSD(analyticalModel_Edited, path_Model, out string _, out string refusal_Edited));
                Assert.Contains("The model has changed", refusal_Edited);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// <b>No culture reaches the digest or the file.</b> The value is taken under the invariant culture and
        /// again under one whose decimal separator is a comma, the model is saved under that culture and
        /// reopened under a third - and the digest is the same throughout. A recorded value is compared on
        /// whatever machine reopens the model, so anything culture-dependent here would refuse valid results
        /// across a locale boundary.
        /// </summary>
        [Fact]
        public void Fingerprint_DoesNotDependOnTheCultureItIsTakenOrSavedUnder()
        {
            string directory = NewDirectory();
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                AnalyticalModel analyticalModel = WithWeather(Model("run"), GroundTemperature(1.25, 8.5));
                analyticalModel.SetValue(AnalyticalModelParameter.NorthAngle, 1.5);

                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                string fingerprint = SimulationResultProvenance.Fingerprint(analyticalModel);

                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel));

                AnalyticalModel analyticalModel_Reopened = SaveAndReopen(analyticalModel, Path.Combine(directory, "run.sam"));

                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                Assert.Equal(fingerprint, SimulationResultProvenance.Fingerprint(analyticalModel_Reopened));
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
                Directory.Delete(directory, true);
            }
        }
    }
}
