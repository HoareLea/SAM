// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// SAM#146 (Phase-2 audit B0): one <see cref="ParameterSet"/> per assembly name on an object. SAM.Analytical has
    /// no [assembly: Guid], so its set GUID is the per-build module version id; saved models carry one
    /// "SAM.Analytical" set per build that imported from Tas (SAM_Tas Query.UpdateT3D adds a set built with the
    /// current GUID), and a reader on another build used to take the value from the OLDEST set holding the key.
    /// The shapes below are the real ones from C:\TasOut\final1b\open_out.sam, Bathroom_2.
    /// </summary>
    public class ParameterSetIdentityTests
    {
        private static readonly Guid SpaceGuid = new Guid("e2c8a683-410d-42fd-92cb-bcf1e87e5a3a");

        // The three SAM.Analytical set GUIDs on Bathroom_2: authoring, first Tas run, latest Tas run.
        private static readonly Guid Guid_Authoring = new Guid("990f1c57-aca9-4e09-8c0e-d5d538b111b1");
        private static readonly Guid Guid_Run1 = new Guid("cc94e7a1-faf2-40de-b400-82b7e8a573fc");
        private static readonly Guid Guid_Run2 = new Guid("feae3a10-9c31-4413-8d20-797b65029737");
        private static readonly Guid Guid_Tas = new Guid("6a0a409a-2f23-4f8a-b3da-00cc22996164");

        private const string Name_DesignHeatingLoad = "Design Heating Load";
        private const string Name_DesignCoolingLoad = "Design Cooling Load";
        private const double Bathroom2_DesignHeatingLoad = 1139.87451171875;

        private static string Name_Analytical => Core.Query.Name(typeof(Space).Assembly);

        private static Guid Guid_CurrentBuild => Core.Query.Guid(typeof(Space).Assembly);

        private static JsonObject Set(Guid guid, string name, params (string Name, object Value)[] values)
        {
            ParameterSet parameterSet = new ParameterSet(guid, name);
            foreach ((string Name, object Value) value in values)
            {
                switch (value.Value)
                {
                    case double @double:
                        parameterSet.Add(value.Name, @double);
                        break;
                    case bool @bool:
                        parameterSet.Add(value.Name, @bool);
                        break;
                    case string @string:
                        parameterSet.Add(value.Name, @string);
                        break;
                    default:
                        throw new ArgumentException(value.Name);
                }
            }

            return parameterSet.ToJsonObject();
        }

        private static JsonObject Set_Authoring()
        {
            return Set(Guid_Authoring, Name_Analytical, ("Level Name", "Level 0"), ("Area", 25.0), ("Volume", 100.0));
        }

        private static JsonObject Set_Tas(double designHeatingLoad, double designCoolingLoad, Guid guid)
        {
            return Set(guid, Name_Analytical, ("IsUsed", true), ("Facing External", true), ("Facing External Glazing", false), (Name_DesignHeatingLoad, designHeatingLoad), (Name_DesignCoolingLoad, designCoolingLoad));
        }

        /// <summary>
        /// A space as a saved file holds it, with <paramref name="parameterSets"/> in stored order.
        /// </summary>
        private static Space Load(params JsonObject[] parameterSets)
        {
            JsonObject jsonObject = new Space(SpaceGuid, "Bathroom_2", new Point3D(0, 0, 0)).ToJsonObject();
            jsonObject["ParameterSets"] = new JsonArray(parameterSets.Select(x => (JsonNode)x).ToArray());

            return Core.Create.IJSAMObject<Space>(jsonObject.ToJsonString());
        }

        private static Space Bathroom2()
        {
            return Load(Set_Authoring(), Set_Tas(0.0, 0.0, Guid_Run1), Set(Guid_Tas, "SAM.Analytical.Tas.dll", ("Zone Guid", "{5C8B92E4-6FC6-4DE3-B2AA-C77E131E3AEB}")), Set_Tas(Bathroom2_DesignHeatingLoad, 0.0, Guid_Run2));
        }

        private static int Count(ParameterizedSAMObject parameterizedSAMObject, string name)
        {
            return parameterizedSAMObject.GetParameterSets()?.Count(x => x.Name == name) ?? 0;
        }

        private static double DesignHeatingLoad(Space space)
        {
            Assert.True(space.TryGetValue(SpaceParameter.DesignHeatingLoad, out double value));
            return value;
        }

        // ---------- read ----------

        [Fact]
        public void Bathroom2_LegacyDuplicateSets_ReadTheLatestRunsLoad()
        {
            Space space = Bathroom2();

            Assert.Equal(Bathroom2_DesignHeatingLoad, DesignHeatingLoad(space));
            Assert.True(space.TryGetValue(SpaceParameter.DesignCoolingLoad, out double designCoolingLoad));
            Assert.Equal(0.0, designCoolingLoad);

            // One SAM.Analytical set; the authoring values and the other assembly's set survive.
            Assert.Equal(1, Count(space, Name_Analytical));
            Assert.Equal(1, Count(space, "SAM.Analytical.Tas.dll"));
            Assert.Equal(25.0, space.GetValue<double>(SpaceParameter.Area));
            Assert.Equal("Level 0", space.GetValue<string>(SpaceParameter.LevelName));
            Assert.True(space.TryGetValue("Zone Guid", out string zoneGuid));
            Assert.Equal("{5C8B92E4-6FC6-4DE3-B2AA-C77E131E3AEB}", zoneGuid);

            // The merged set keeps the first set's identity and position.
            ParameterSet parameterSet = space.GetParameterSets()[0];
            Assert.Equal(Name_Analytical, parameterSet.Name);
            Assert.Equal(Guid_Authoring, parameterSet.Guid);
        }

        [Fact]
        public void Bathroom2_ReadIsTheSameThroughEveryAccessor()
        {
            Space space = Bathroom2();

            Assert.Equal(Bathroom2_DesignHeatingLoad, space.GetValue<double>(SpaceParameter.DesignHeatingLoad));
            Assert.True(space.TryGetValue(Name_DesignHeatingLoad, out double byName));
            Assert.Equal(Bathroom2_DesignHeatingLoad, byName);
            Assert.True(Core.Query.TryGetValue(space, Name_DesignHeatingLoad, typeof(Space).Assembly, out object byAssembly));
            Assert.Equal(Bathroom2_DesignHeatingLoad, byAssembly);
        }

        [Fact]
        public void CurrentSetOnly_ReadsAndRoundTripsUnchanged()
        {
            Space space = new Space(SpaceGuid, "Bathroom_2", new Point3D(0, 0, 0));
            space.SetValue(SpaceParameter.Area, 25.0);
            space.SetValue(SpaceParameter.DesignHeatingLoad, 500.0);

            string json = Core.Convert.ToString(space);
            Space result = Core.Create.IJSAMObject<Space>(json);

            Assert.Equal(500.0, DesignHeatingLoad(result));
            Assert.Equal(1, Count(result, Name_Analytical));
            Assert.Equal(Guid_CurrentBuild, result.GetParameterSets().Single().Guid);
            RoundTrip.AssertEquivalent(json, Core.Convert.ToString(result));
        }

        [Fact]
        public void NoDuplicates_FileLoadsUnchanged()
        {
            JsonObject[] parameterSets = { Set_Authoring(), Set(Guid_Tas, "SAM.Analytical.Tas.dll", ("Zone Guid", "{Z}")) };
            string json_Before = new JsonArray(parameterSets.Select(x => (JsonNode)x.DeepClone()).ToArray()).ToJsonString();

            Space space = Load(parameterSets);

            string json_After = space.ToJsonObject()["ParameterSets"]!.ToJsonString();
            Assert.Equal(json_Before, json_After);
        }

        [Fact]
        public void LegacySetOnly_ReadsItsValue()
        {
            Space space = Load(Set(Guid_Run1, Name_Analytical, ("Area", 25.0), (Name_DesignHeatingLoad, 750.0)));

            Assert.NotEqual(Guid_CurrentBuild, Guid_Run1);
            Assert.Equal(750.0, DesignHeatingLoad(space));
        }

        [Fact]
        public void LegacyThenCurrentBuildSet_ReadsTheCurrentBuildSet()
        {
            Space space = Load(Set_Authoring(), Set_Tas(0.0, 0.0, Guid_Run1), Set_Tas(1200.0, 0.0, Guid_CurrentBuild));

            Assert.Equal(1200.0, DesignHeatingLoad(space));
            Assert.Equal(1, Count(space, Name_Analytical));
        }

        [Fact]
        public void StoredOrder_NotTheGuid_DecidesPrecedence()
        {
            // A build GUID is a hash of one compilation and carries no authority: a set stored later overrides an
            // earlier one even when the earlier one happens to have this build's GUID.
            Space space = Load(Set_Tas(1200.0, 0.0, Guid_CurrentBuild), Set_Tas(900.0, 0.0, Guid_Run1));

            Assert.Equal(900.0, DesignHeatingLoad(space));
        }

        [Fact]
        public void MultipleLegacySets_LastStoredValueWins_EarlierOnlyKeysSurvive()
        {
            Space space = Load(
                Set(Guid_Authoring, Name_Analytical, ("Area", 25.0), (Name_DesignHeatingLoad, 100.0)),
                Set(Guid_Run1, Name_Analytical, (Name_DesignHeatingLoad, 200.0), ("Volume", 75.0)),
                Set(Guid_Run2, Name_Analytical, (Name_DesignHeatingLoad, 300.0)));

            Assert.Equal(300.0, DesignHeatingLoad(space));
            Assert.Equal(25.0, space.GetValue<double>(SpaceParameter.Area));
            Assert.Equal(75.0, space.GetValue<double>(SpaceParameter.Volume));
        }

        [Fact]
        public void ExplicitZero_IsKeptAsZero()
        {
            Space space = Load(Set_Authoring(), Set_Tas(Bathroom2_DesignHeatingLoad, 0.0, Guid_Run1), Set_Tas(0.0, 0.0, Guid_Run2));

            Assert.True(space.HasValue(SpaceParameter.DesignHeatingLoad));
            Assert.Equal(0.0, DesignHeatingLoad(space));
        }

        [Fact]
        public void MissingLoad_StaysMissing_NotZero()
        {
            Space space = Load(Set_Authoring(), Set(Guid_Run1, Name_Analytical, ("IsUsed", true)), Set(Guid_Tas, "SAM.Analytical.Tas.dll", ("Zone Guid", "{Z}")));

            Assert.False(space.HasValue(SpaceParameter.DesignHeatingLoad));
            Assert.False(space.TryGetValue(SpaceParameter.DesignHeatingLoad, out double _));
            Assert.False(space.TryGetValue(SpaceParameter.DesignCoolingLoad, out double _));
        }

        [Fact]
        public void DifferentNames_AreNotMerged()
        {
            Space space = Load(Set_Authoring(), Set(Guid_Tas, "SAM.Analytical.Tas.dll", (Name_DesignHeatingLoad, 42.0)));

            Assert.Equal(2, space.GetParameterSets().Count);
            Assert.False(space.GetParameterSets()[0].Contains(Name_DesignHeatingLoad));
            // The cross-assembly fallback is unchanged: the only set holding the key answers.
            Assert.Equal(42.0, DesignHeatingLoad(space));
        }

        // ---------- write ----------

        [Fact]
        public void TasRunsOnNewBuilds_UpdateTheExistingSet_InsteadOfAppending()
        {
            Space space = Bathroom2();

            // What SAM_Tas Query.UpdateT3D does on every workflow run: add a set built from the TAS3D zone with the
            // running build's GUID, then write the design loads (Modify.UpdateDesignLoads).
            foreach ((Guid Guid, double Load) run in new[] { (Guid_CurrentBuild, 1500.0), (Guid.NewGuid(), 1600.0), (Guid.NewGuid(), 0.0) })
            {
                ParameterSet parameterSet_Zone = new ParameterSet(run.Guid, Name_Analytical);
                parameterSet_Zone.Add("IsUsed", true);

                Space space_New = new Space(space);
                space_New.Add(parameterSet_Zone);
                Assert.True(space_New.SetValue(SpaceParameter.DesignHeatingLoad, run.Load));

                space = Core.Create.IJSAMObject<Space>(Core.Convert.ToString(space_New));

                Assert.Equal(1, Count(space, Name_Analytical));
                Assert.Equal(run.Load, DesignHeatingLoad(space));
            }
        }

        [Fact]
        public void SetValue_OnALegacyFile_WritesTheOneSet()
        {
            Space space = Bathroom2();

            Assert.True(space.SetValue(SpaceParameter.DesignHeatingLoad, 250.0));
            Assert.True(space.SetValue(SpaceParameter.DesignCoolingLoad, 0.0));

            Space result = RoundTrip.Once(space);
            Assert.Equal(1, Count(result, Name_Analytical));
            Assert.Equal(250.0, DesignHeatingLoad(result));
            Assert.True(result.TryGetValue(SpaceParameter.DesignCoolingLoad, out double designCoolingLoad));
            Assert.Equal(0.0, designCoolingLoad);
        }

        [Fact]
        public void ParameterSetsConstructor_MergesSameNameSets()
        {
            ParameterSet parameterSet_1 = new ParameterSet(Guid_Run1, Name_Analytical);
            parameterSet_1.Add(Name_DesignHeatingLoad, 1.0);
            parameterSet_1.Add("Area", 25.0);
            ParameterSet parameterSet_2 = new ParameterSet(Guid_Run2, Name_Analytical);
            parameterSet_2.Add(Name_DesignHeatingLoad, 2.0);

            ParameterizedSAMObject parameterizedSAMObject = new ParameterizedSAMObject(new List<ParameterSet> { parameterSet_1, parameterSet_2 });

            ParameterSet result = Assert.Single(parameterizedSAMObject.GetParameterSets());
            Assert.Equal(Guid_Run1, result.Guid);
            Assert.Equal(2.0, result.ToDouble(Name_DesignHeatingLoad));
            Assert.Equal(25.0, result.ToDouble("Area"));
        }

        [Fact]
        public void Add_MatchesByGuidFirst_ThenByName_NeverByEmptyName()
        {
            List<ParameterSet> parameterSets = new List<ParameterSet>();

            Assert.True(Core.Modify.Add(parameterSets, new ParameterSet(Guid_Run1, "A")));
            Assert.True(Core.Modify.Add(parameterSets, new ParameterSet(Guid_Run1, "B")));   // same GUID: merged
            Assert.True(Core.Modify.Add(parameterSets, new ParameterSet(Guid_Run2, "A")));   // same name: merged
            Assert.True(Core.Modify.Add(parameterSets, new ParameterSet(Guid.NewGuid(), (string)null)));
            Assert.True(Core.Modify.Add(parameterSets, new ParameterSet(Guid.NewGuid(), (string)null)));
            Assert.True(Core.Modify.Add(parameterSets, new ParameterSet(Guid.NewGuid(), string.Empty)));

            Assert.Equal(4, parameterSets.Count);
            Assert.Equal("A", parameterSets[0].Name);
        }
    }
}
