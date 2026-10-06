// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>OverheatingScenario</c>'s derived identity.
    /// <para>
    /// The whole class exists so that stating the same Approved Document O assessment twice - on two
    /// machines, before and after a save, in two different orders - produces one identity, and stating a
    /// different assessment produces a different one. Everything below is one half of that: either
    /// something that must not change the key, or something that must.
    /// </para>
    /// <para>
    /// Fixture is the real validation shape: the three-flat block with a communal corridor that the TAS run
    /// used, with MVRE - SAM's heat-recovery ventilation - on the flats.
    /// </para>
    /// </summary>
    public class OverheatingScenarioTests
    {
        private static readonly Guid guid_Flat_1 = new("11111111-1111-4111-8111-111111111111");
        private static readonly Guid guid_Flat_2 = new("22222222-2222-4222-8222-222222222222");
        private static readonly Guid guid_Corridor = new("33333333-3333-4333-8333-333333333333");

        // ---------------------------------------------------------------------------------------------
        // 1. The same engineering scenario is one scenario
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Two scenarios built independently from the same engineering statement derive one key. Nothing
        /// is shared between the two constructions - separate <c>SystemTemplate</c>s, separate assumption
        /// objects - so this fails the moment any part of the identity comes from the instance rather than
        /// from what it says.
        /// </summary>
        [Fact]
        public void SameEngineeringScenario_DerivesTheSameKey()
        {
            Assert.Equal(Scenario().Key, Scenario().Key);
        }

        /// <summary>
        /// The order the assumptions were stated in is not part of the assessment. Two engineers who set
        /// the same three assumptions in different orders described the same thing.
        /// </summary>
        [Fact]
        public void AssumptionOrder_DoesNotChangeTheKey()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_1 = new();
            overheatingOperatingAssumptions_1.Set("Openings", "Unrestricted");
            overheatingOperatingAssumptions_1.Set("MechanicalRate_Lps", 21.0);
            overheatingOperatingAssumptions_1.Set("SummerBypass", false);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_2 = new();
            overheatingOperatingAssumptions_2.Set("SummerBypass", false);
            overheatingOperatingAssumptions_2.Set("MechanicalRate_Lps", 21.0);
            overheatingOperatingAssumptions_2.Set("Openings", "Unrestricted");

            Assert.Equal(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_1).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_2).Key);
        }

        /// <summary>
        /// A key derived today equals the one derived by the code that first wrote this test. Everything
        /// else here compares two keys made by the same build, which cannot catch a change to the encoding
        /// itself - a reordered component, a dropped length prefix, a switch to a different hash - all of
        /// which would silently re-key every assessment ever recorded.
        /// <para>
        /// <b>This value is only ever regenerated together with a deliberate bump of
        /// <c>OverheatingScenario.IdentitySchema</c>.</b> A change here without one is a bug, not a
        /// stale expectation.
        /// </para>
        /// </summary>
        [Fact]
        public void Key_IsStableAcrossBuilds()
        {
            Assert.Equal("OverheatingScenario:v1", OverheatingScenario.IdentitySchema);
            Assert.Equal(new Guid("e81d5d9f-3672-801d-933b-f2e3c19bb284"), Scenario().Key);

            //Version 8 and the RFC 4122 variant, so a derived key is visibly not a model guid.
            byte[] bytes = Scenario().Key.ToByteArray();

            Assert.Equal(0x80, bytes[7] & 0xF0);
            Assert.Equal(0x80, bytes[8] & 0xC0);
        }

        // ---------------------------------------------------------------------------------------------
        // 2. Serialisation
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// A scenario written to JSON and read back is the same assessment. The key survives because it is
        /// re-derived from the state, not because it was saved.
        /// </summary>
        [Fact]
        public void JsonRoundTrip_DerivesTheSameKey()
        {
            OverheatingScenario overheatingScenario = Scenario();
            overheatingScenario.Name = "Flat 1 - base";
            overheatingScenario.Source = "SAM_zoningAM_v2zonesisDomestic.sam";

            OverheatingScenario overheatingScenario_RoundTrip = Core.Create.IJSAMObject<OverheatingScenario>(overheatingScenario.ToJsonObject().ToJsonString());

            Assert.NotNull(overheatingScenario_RoundTrip);
            Assert.Equal(overheatingScenario.Key, overheatingScenario_RoundTrip.Key);

            Assert.Equal(overheatingScenario.Scope, overheatingScenario_RoundTrip.Scope);
            Assert.Equal(overheatingScenario.ZoneGuid, overheatingScenario_RoundTrip.ZoneGuid);
            Assert.Equal(overheatingScenario.Iteration, overheatingScenario_RoundTrip.Iteration);
            Assert.Equal(overheatingScenario.Name, overheatingScenario_RoundTrip.Name);
            Assert.Equal(overheatingScenario.Source, overheatingScenario_RoundTrip.Source);
            Assert.Equal("MVRE", overheatingScenario_RoundTrip.SystemTemplate?.Ventilation);
            Assert.Equal("Unrestricted", overheatingScenario_RoundTrip.OperatingAssumptions.Value("Openings"));
        }

        /// <summary>
        /// <b>The key is never written.</b> A stored key is a second copy of an identity that is supposed
        /// to have one source, and a file carrying one that disagrees with its own contents would be
        /// believed. There is nothing to disagree with if nothing is stored.
        /// </summary>
        [Fact]
        public void Key_IsNeverSerialised()
        {
            OverheatingScenario overheatingScenario = Scenario();

            JsonObject jsonObject = overheatingScenario.ToJsonObject();

            foreach (KeyValuePair<string, JsonNode> keyValuePair in jsonObject)
            {
                Assert.DoesNotContain("key", keyValuePair.Key, StringComparison.OrdinalIgnoreCase);
            }

            Assert.DoesNotContain(overheatingScenario.Key.ToString("D"), jsonObject.ToJsonString(), StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------------------------------------------
        // 3-6. What must change the key
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Two flats called the same thing are two flats.</b> Name is not identity and the design zone
        /// guid is, so an identical name over a different dwelling is a different assessment - and the
        /// reverse holds too: renaming a flat does not orphan its assessment.
        /// </summary>
        [Fact]
        public void SameNameDifferentDwelling_DerivesADifferentKey()
        {
            OverheatingScenario overheatingScenario_1 = Scenario(guid_Zone: guid_Flat_1);
            overheatingScenario_1.Name = "Flat 2";

            OverheatingScenario overheatingScenario_2 = Scenario(guid_Zone: guid_Flat_2);
            overheatingScenario_2.Name = "Flat 2";

            Assert.NotEqual(overheatingScenario_1.Key, overheatingScenario_2.Key);
        }

        /// <summary>The other direction: a rename is not a new assessment.</summary>
        [Fact]
        public void DifferentNameSameDwelling_DerivesTheSameKey()
        {
            OverheatingScenario overheatingScenario_1 = Scenario();
            overheatingScenario_1.Name = "Flat 1";

            OverheatingScenario overheatingScenario_2 = Scenario();
            overheatingScenario_2.Name = "Apartment 1 (revised)";

            Assert.Equal(overheatingScenario_1.Key, overheatingScenario_2.Key);
        }

        /// <summary>
        /// The scope is in the key in its own right, so a dwelling assessment and a common-space assessment
        /// over the same zone guid can never collide - which is the mechanism behind "the corridor is
        /// assessed but never attributed to a dwelling".
        /// </summary>
        [Fact]
        public void DifferentScope_DerivesADifferentKey()
        {
            Assert.NotEqual(
                Scenario(partOAssessmentScope: PartOAssessmentScope.Dwelling, guid_Zone: guid_Corridor).Key,
                Scenario(partOAssessmentScope: PartOAssessmentScope.CommonSpace, guid_Zone: guid_Corridor).Key);
        }

        /// <summary>
        /// The same dwelling at two mitigation stages is two engineering answers, and they must not share
        /// an identity - otherwise the second overwrites the first.
        /// </summary>
        [Fact]
        public void DifferentIteration_DerivesADifferentKey()
        {
            List<Guid> guids = [];

            foreach (PartOIteration partOIteration in Enum.GetValues(typeof(PartOIteration)))
            {
                Guid guid = Scenario(partOIteration: partOIteration).Key;

                Assert.DoesNotContain(guid, guids);
                guids.Add(guid);
            }

            Assert.Equal(Enum.GetValues(typeof(PartOIteration)).Length, guids.Count);
        }

        /// <summary>
        /// <b>Every field of the system identity participates - all six.</b> Asserted one field at a time
        /// rather than by changing the template wholesale, because a derivation that quietly dropped, say,
        /// <c>Controls</c> would still pass a single wholesale comparison and would then merge two
        /// genuinely different assessments.
        /// </summary>
        [Fact]
        public void DifferentSystemTemplate_DerivesADifferentKey()
        {
            Guid guid = Scenario().Key;

            Assert.NotEqual(guid, Scenario(systemTemplate: Template("NV")).Key);
            Assert.NotEqual(guid, Scenario(systemTemplate: Template(heating: "UH")).Key);
            Assert.NotEqual(guid, Scenario(systemTemplate: Template(cooling: "CH1")).Key);
            Assert.NotEqual(guid, Scenario(systemTemplate: Template(plantRoom: "PR2")).Key);
            Assert.NotEqual(guid, Scenario(systemTemplate: Template(controls: "CTL2")).Key);
            Assert.NotEqual(guid, Scenario(systemTemplate: Template(version: "2")).Key);

            //And no system stated at all is not the same as this one. Built directly rather than through
            //the fixture, whose null means "the usual one".
            Assert.NotEqual(guid, new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Flat_1, PartOIteration.Undefined, null, Assumptions()).Key);

            //A template with nothing stated in it IS no template - "no system" and "a system nothing is
            //said about" are the same statement, and normalising them here is what stops them being two.
            Assert.Equal(
                new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Flat_1, PartOIteration.Undefined, null, Assumptions()).Key,
                new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Flat_1, PartOIteration.Undefined, new SystemTemplate(), Assumptions()).Key);
        }

        /// <summary>
        /// An operating assumption is identity-defining however it differs - a changed value, a renamed
        /// assumption, or one more of them. All three are a different assessment of the same fabric.
        /// </summary>
        [Fact]
        public void DifferentOperatingAssumption_DerivesADifferentKey()
        {
            Guid guid = Scenario().Key;

            OverheatingOperatingAssumptions overheatingOperatingAssumptions;

            overheatingOperatingAssumptions = Assumptions();
            overheatingOperatingAssumptions.Set("Openings", "Restricted");
            Assert.NotEqual(guid, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions).Key);

            overheatingOperatingAssumptions = Assumptions();
            overheatingOperatingAssumptions.Remove("Openings");
            overheatingOperatingAssumptions.Set("OpeningBehaviour", "Unrestricted");
            Assert.NotEqual(guid, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions).Key);

            overheatingOperatingAssumptions = Assumptions();
            overheatingOperatingAssumptions.Set("Boost", true);
            Assert.NotEqual(guid, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions).Key);

            overheatingOperatingAssumptions = Assumptions();
            overheatingOperatingAssumptions.Set("MechanicalRate_Lps", 21.5);
            Assert.NotEqual(guid, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions).Key);
        }

        /// <summary>
        /// <b>Concatenation is ambiguous and length-prefixing is why this passes.</b> An assumption called
        /// <c>AB</c> with value <c>C</c> and one called <c>A</c> with value <c>BC</c> flatten to the same
        /// characters. They are not the same assumption, and a derivation that simply joined its
        /// components would give them one key.
        /// </summary>
        [Fact]
        public void ComponentsThatFlattenAlike_DeriveDifferentKeys()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_1 = new();
            overheatingOperatingAssumptions_1.Set("AB", "C");

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_2 = new();
            overheatingOperatingAssumptions_2.Set("A", "BC");

            Assert.NotEqual(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_1).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_2).Key);

            //The same trap across the system fields: "MV" + "RE1" is not "MVRE" + "1".
            Assert.NotEqual(Scenario(systemTemplate: Template("MV", version: "RE1")).Key, Scenario(systemTemplate: Template("MVRE", version: "1")).Key);
        }

        /// <summary>
        /// An unstated component and one stated as blank are different statements, and the derivation keeps
        /// them apart.
        /// </summary>
        [Fact]
        public void UnstatedAndBlank_DeriveDifferentKeys()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_Blank = new();
            overheatingOperatingAssumptions_Blank.Set("Openings", string.Empty);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_Unstated = new();

            Assert.NotEqual(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_Blank).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_Unstated).Key);
        }

        // ---------------------------------------------------------------------------------------------
        // 7-8. What must NOT change the key
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Provenance is not identity.</b> Where an answer came from is not part of what the question
        /// was, so re-running the same assessment from a different model file, workflow or user is the same
        /// assessment - which is what makes it comparable with the earlier run rather than a new one.
        /// </summary>
        [Fact]
        public void ProvenanceChange_DerivesTheSameKey()
        {
            OverheatingScenario overheatingScenario = Scenario();

            Guid guid = overheatingScenario.Key;

            overheatingScenario.Source = "SAM_zoningAM_v2zonesisDomestic.sam";
            Assert.Equal(guid, overheatingScenario.Key);

            overheatingScenario.Source = "Rebuilt by hand, 2026-08-08";
            Assert.Equal(guid, overheatingScenario.Key);

            overheatingScenario.Name = "anything at all";
            Assert.Equal(guid, overheatingScenario.Key);
        }

        /// <summary>
        /// <b>Routing is not identity.</b> The same scenario answered by a simple TSD run and by a full
        /// HVAC TPD run is one assessment computed two ways - that boundary is the point of Iteration 0's
        /// architecture, and a key that moved with the route would destroy it.
        /// <para>
        /// <b>What this proves and what it does not.</b> There is no routing member to change - that is the
        /// design - so the only place a route can be recorded at all is provenance, and this asserts that
        /// recording it there moves nothing. The absence itself is asserted structurally in
        /// <see cref="ScenarioType_HasNoEngineOrRoutingMember"/>, which is where the real guard lives.
        /// </para>
        /// </summary>
        [Fact]
        public void RoutingChange_DerivesTheSameKey()
        {
            OverheatingScenario overheatingScenario = Scenario();

            Guid guid = overheatingScenario.Key;

            foreach (string text in new[] { "TAS 9.5.7.0 TSD", "TAS 9.5.7.0 TPD", @"C:\SAM_daily\2027-08-03-HVAC\000000_SAM_AnalyticalModel.tsd" })
            {
                overheatingScenario.Source = text;
                Assert.Equal(guid, overheatingScenario.Key);
            }
        }

        /// <summary>
        /// The structural half of the same promise. <c>SAM.Analytical</c> is engine-free and a scenario
        /// describes intent, so no member of it - public or private - may name an engine, a route or a
        /// file.
        /// <para>
        /// The assembly reference direction is deliberately <b>not</b> asserted here: <c>SAM.Analytical</c>
        /// referencing <c>SAM.Analytical.Tas</c> would be circular and cannot compile, so a test of it
        /// could never fail and would only look like cover. The member names are the thing that could
        /// actually go wrong.
        /// </para>
        /// </summary>
        [Fact]
        public void ScenarioType_HasNoEngineOrRoutingMember()
        {
            string[] forbidden = ["tas", "tsd", "tpd", "path", "file", "directory", "weather", "simulat", "result"];

            foreach (MemberInfo memberInfo in typeof(OverheatingScenario).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                foreach (string text in forbidden)
                {
                    Assert.False(memberInfo.Name.ToLowerInvariant().Contains(text), string.Format("OverheatingScenario declares '{0}', which names {1}. A scenario states intent only.", memberInfo.Name, text));
                }
            }
        }

        // ---------------------------------------------------------------------------------------------
        // 9. Encoding
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Non-ASCII values stay distinct - and the first line of this test is why the derivation does
        /// not use <c>Core.Query.ComputeHash</c>.</b> That helper encodes ASCII, which maps every character
        /// outside it to <c>?</c>, so under it "café" and "cafè" are one string. Zone and assumption text
        /// comes from engineers' models, and an accented name is not exotic.
        /// </summary>
        [Fact]
        public void NonAsciiValues_RemainDistinct()
        {
            string text_1 = "caf\u00E9";
            string text_2 = "caf\u00E8";

            //The positive control. If this ever stops being true, ComputeHash was fixed and this test's
            //premise - not its conclusion - has changed.
            Assert.Equal(Core.Query.ComputeHash(text_1), Core.Query.ComputeHash(text_2));

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_1 = new();
            overheatingOperatingAssumptions_1.Set("Openings", text_1);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_2 = new();
            overheatingOperatingAssumptions_2.Set("Openings", text_2);

            Assert.NotEqual(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_1).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_2).Key);

            //The same, in an assumption's name rather than its value.
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_3 = new();
            overheatingOperatingAssumptions_3.Set(text_1, "Unrestricted");

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_4 = new();
            overheatingOperatingAssumptions_4.Set(text_2, "Unrestricted");

            Assert.NotEqual(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_3).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_4).Key);

            //And it survives JSON, which is where an encoding mistake would otherwise appear.
            OverheatingScenario overheatingScenario = Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_1);
            Assert.Equal(overheatingScenario.Key, Core.Create.IJSAMObject<OverheatingScenario>(overheatingScenario.ToJsonObject().ToJsonString()).Key);
        }

        /// <summary>
        /// A numeric assumption is formatted invariantly, <b>asserted under a culture that would otherwise
        /// break it</b>. Asserting <c>"21.5"</c> on an en-* machine proves nothing at all - it passes
        /// identically whether or not the formatter is told to be invariant, so a refactor that dropped the
        /// <c>InvariantCulture</c> argument would stay green on CI and every English machine and split one
        /// assessment in two only for a German-configured engineer.
        /// </summary>
        [Fact]
        public void NumericAssumption_IsFormattedInvariantly()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                OverheatingOperatingAssumptions overheatingOperatingAssumptions = new();
                overheatingOperatingAssumptions.Set("MechanicalRate_Lps", 21.5);

                Assert.Equal("21.5", overheatingOperatingAssumptions.Value("MechanicalRate_Lps"));

                //And the whole way through to the key.
                Assert.Equal(Scenario().Key, Scenario().Key);
                Assert.Equal(new Guid("e81d5d9f-3672-801d-933b-f2e3c19bb284"), Scenario().Key);
            }
            finally
            {
                CultureInfo.CurrentCulture = cultureInfo;
            }
        }

        /// <summary>
        /// <b>A numeric assumption reads the same under every .NET this assembly loads on.</b> The obvious
        /// formats do not: <c>"R"</c> and <c>"G17"</c> both changed meaning in .NET Core 3.0, so
        /// <c>2.0/3.0</c> is <c>0.66666666666666663</c> under .NET Framework and
        /// <c>0.6666666666666666</c> under .NET 8. <c>SAM.Analytical</c> is <c>netstandard2.0</c> so that it
        /// loads under both, and SAM has live .NET Framework consumers - so a round-trip format would mean
        /// the Revit-side process and the WPF-side process deriving two keys for one stated assessment.
        /// <para>
        /// Pinned to literal text rather than to a round trip, because a round-trip assertion would itself
        /// be satisfied by either runtime's answer.
        /// </para>
        /// </summary>
        [Fact]
        public void NumericAssumption_ReadsTheSameOnEveryRuntime()
        {
            Assert.Equal("0.666666667", OverheatingOperatingAssumptions.Text(2.0 / 3.0));
            Assert.Equal("0.333333333", OverheatingOperatingAssumptions.Text(1.0 / 3.0));
            Assert.Equal("0.6822872", OverheatingOperatingAssumptions.Text(0.6822871999174));

            Assert.Equal("21", OverheatingOperatingAssumptions.Text(21.0));
            Assert.Equal("21.5", OverheatingOperatingAssumptions.Text(21.5));

            //Negative zero is the same assumption as zero. Only the formatter tells them apart, and on .NET
            //5+ it does.
            Assert.Equal(OverheatingOperatingAssumptions.Text(0.0), OverheatingOperatingAssumptions.Text(-0.0));
            Assert.Equal("0", OverheatingOperatingAssumptions.Text(-0.0));

            //Written out by name rather than through ToString, whose symbols are a culture's business.
            Assert.Equal("NaN", OverheatingOperatingAssumptions.Text(double.NaN));
            Assert.Equal("Infinity", OverheatingOperatingAssumptions.Text(double.PositiveInfinity));
            Assert.Equal("-Infinity", OverheatingOperatingAssumptions.Text(double.NegativeInfinity));

            //And a rate stated to nine places is still distinguishable from one that differs there.
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_1 = new();
            overheatingOperatingAssumptions_1.Set("MechanicalRate_Lps", 2.0 / 3.0);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_2 = new();
            overheatingOperatingAssumptions_2.Set("MechanicalRate_Lps", 0.666666668);

            Assert.NotEqual(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_1).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_2).Key);
        }

        /// <summary>
        /// The same accented character written as one code point and as a letter plus a combining accent is
        /// one name to everyone reading it, and text typed on macOS is routinely the second form. Without
        /// normalisation they are different UTF-8 bytes and therefore two assessments of one dwelling.
        /// </summary>
        [Fact]
        public void UnicodeNormalisationForms_AreOneName()
        {
            //Written as escapes, not as accented source text, so an editor re-saving this file in a
            //different normalisation form cannot quietly turn this into a comparison of two identical
            //strings.
            string text_Composed = "caf\u00E9";
            string text_Decomposed = "cafe\u0301";

            Assert.NotEqual(text_Composed, text_Decomposed);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_1 = new();
            overheatingOperatingAssumptions_1.Set("Openings", text_Composed);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_2 = new();
            overheatingOperatingAssumptions_2.Set("Openings", text_Decomposed);

            Assert.Equal(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_1).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_2).Key);
        }

        /// <summary>
        /// <b>Normalising only at the point of hashing is not enough, and this is the case that proves
        /// it.</b> The assumptions are hashed in ordinal name order, and ordinal order runs over raw code
        /// units: composed <c>é</c> is U+00E9 and sorts <b>after</b> <c>f</c>, while the decomposed form
        /// begins with <c>e</c> and sorts <b>before</b> it. So two canonically identical sets of
        /// assumptions would have been hashed in different orders and derived two keys - normalised text,
        /// but in the wrong sequence. The name is therefore normalised on the way into the sorted store,
        /// not on the way out of it.
        /// </summary>
        [Fact]
        public void UnicodeNormalisation_DoesNotChangeCanonicalOrdering()
        {
            //The bare accented character, not a word containing one - a leading "c" would sort both forms
            //to the same side of "f" and the premise below would be vacuous.
            string text_Composed = "\u00E9";
            string text_Decomposed = "e\u0301";

            //The premise: these really do sort on opposite sides of "f".
            Assert.True(string.CompareOrdinal(text_Composed, "f") > 0);
            Assert.True(string.CompareOrdinal(text_Decomposed, "f") < 0);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_1 = new();
            overheatingOperatingAssumptions_1.Set(text_Composed, "Unrestricted");
            overheatingOperatingAssumptions_1.Set("f", "Restricted");

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_2 = new();
            overheatingOperatingAssumptions_2.Set(text_Decomposed, "Unrestricted");
            overheatingOperatingAssumptions_2.Set("f", "Restricted");

            //One name, one order, one key.
            Assert.Equal(overheatingOperatingAssumptions_1.Names, overheatingOperatingAssumptions_2.Names);
            Assert.Equal(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_1).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_2).Key);

            //And it is readable back by either form.
            Assert.Equal("Unrestricted", overheatingOperatingAssumptions_2.Value(text_Composed));
            Assert.Equal("Unrestricted", overheatingOperatingAssumptions_1.Value(text_Decomposed));
            Assert.True(overheatingOperatingAssumptions_1.Contains(text_Decomposed));
            Assert.True(overheatingOperatingAssumptions_2.Remove(text_Composed));
        }

        /// <summary>
        /// <b>A boolean written the typed way and the JSON way is one assumption.</b>
        /// <c>Set(name, false)</c> stores <c>False</c>; taking a JSON <c>false</c> at its literal text
        /// would store <c>false</c> - the same engineering assumption deriving two keys according to which
        /// door it came through. A JSON primitive goes through the same canonicaliser as the setter that
        /// would have written it.
        /// </summary>
        [Fact]
        public void TypedBooleanAndJsonBoolean_DeriveSameKey()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_Typed = new();
            overheatingOperatingAssumptions_Typed.Set("SummerBypass", false);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_Json = new(new JsonObject
            {
                ["Assumptions"] = new JsonObject { ["SummerBypass"] = false }
            });

            Assert.Equal("False", overheatingOperatingAssumptions_Json.Value("SummerBypass"));
            Assert.Equal(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_Typed).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_Json).Key);
        }

        /// <summary>
        /// The same for a number, which would otherwise bypass
        /// <c>OverheatingOperatingAssumptions.Text(double)</c> entirely and be hashed as whatever text the
        /// JSON writer happened to emit.
        /// </summary>
        [Fact]
        public void TypedNumberAndJsonNumber_DeriveSameKey()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_Typed = new();
            overheatingOperatingAssumptions_Typed.Set("MechanicalRate_Lps", 21.0);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_Json = new(new JsonObject
            {
                ["Assumptions"] = new JsonObject { ["MechanicalRate_Lps"] = 21.0 }
            });

            Assert.Equal("21", overheatingOperatingAssumptions_Json.Value("MechanicalRate_Lps"));
            Assert.Equal(Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_Typed).Key, Scenario(overheatingOperatingAssumptions: overheatingOperatingAssumptions_Json).Key);

            //A JSON string that happens to look numeric is text and stays as it is - "21.0" is not "21".
            OverheatingOperatingAssumptions overheatingOperatingAssumptions_Text = new(new JsonObject
            {
                ["Assumptions"] = new JsonObject { ["MechanicalRate_Lps"] = "21.0" }
            });

            Assert.Equal("21.0", overheatingOperatingAssumptions_Text.Value("MechanicalRate_Lps"));
        }

        /// <summary>
        /// An object or an array is refused rather than flattened into text. There is no canonical form for
        /// arbitrary JSON - property order alone would decide the key - and an operating assumption is a
        /// value, not a structure. The one assumption is dropped; the file stays readable.
        /// </summary>
        [Fact]
        public void StructuredJsonAssumption_IsRefusedNotFlattened()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = new(new JsonObject
            {
                ["Assumptions"] = new JsonObject
                {
                    ["Openings"] = "Unrestricted",
                    ["Structured"] = new JsonObject { ["a"] = 1 },
                    ["List"] = new JsonArray(1, 2)
                }
            });

            Assert.Equal(["Openings"], overheatingOperatingAssumptions.Names);
        }

        /// <summary>
        /// <c>SystemTemplate</c>'s property setters strip spaces but its copy and JSON constructors assign
        /// its fields raw, so <c>"MV RE"</c> means <c>MVRE</c> through one door and <c>MV RE</c> through
        /// another. This commit promotes those six fields into an identity, so that inconsistency would
        /// have become two keys for what <c>SystemTemplate</c> itself treats as one system. Normalised at
        /// the scenario's own boundary; the shared serialisation path is left alone.
        /// </summary>
        [Fact]
        public void SystemTemplateWhitespace_DoesNotSplitOneIdentity()
        {
            SystemTemplate systemTemplate_Constructed = new("MV RE", "RAD1", "UC1", "PR1", "CTL1", "1");

            SystemTemplate systemTemplate_Json = new(new JsonObject { ["Ventilation"] = "MV RE", ["Heating"] = "RAD1", ["Cooling"] = "UC1", ["PlantRoom"] = "PR1", ["Controls"] = "CTL1", ["Version"] = "1" });

            //The two doors really do disagree - the premise of this test, not its conclusion.
            Assert.Equal("MVRE", systemTemplate_Constructed.Ventilation);
            Assert.Equal("MV RE", systemTemplate_Json.Ventilation);

            Assert.Equal(Scenario().Key, Scenario(systemTemplate: systemTemplate_Constructed).Key);
            Assert.Equal(Scenario().Key, Scenario(systemTemplate: systemTemplate_Json).Key);
        }

        // ---------------------------------------------------------------------------------------------
        // 10. The key cannot go stale
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>No mutable input can leave the key describing something the scenario no longer says.</b> Both
        /// objects a scenario is built from are mutable, so this changes every one of them - the instances
        /// handed to the constructor, and the instances read back off the properties - and the key does not
        /// move, because none of them is the object it is derived from.
        /// </summary>
        [Fact]
        public void MutableIdentityInput_CannotLeaveAStaleKey()
        {
            SystemTemplate systemTemplate = Template();
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = Assumptions();

            OverheatingScenario overheatingScenario = new(PartOAssessmentScope.Dwelling, guid_Flat_1, PartOIteration.Undefined, systemTemplate, overheatingOperatingAssumptions);

            Guid guid = overheatingScenario.Key;

            //The caller's own instances.
            systemTemplate.Ventilation = "NV";
            systemTemplate.Cooling = "CH1";
            overheatingOperatingAssumptions.Set("Openings", "Restricted");
            overheatingOperatingAssumptions.Set("Boost", true);

            Assert.Equal(guid, overheatingScenario.Key);

            //And what the properties hand back.
            overheatingScenario.SystemTemplate.Ventilation = "NV";
            overheatingScenario.OperatingAssumptions.Set("Openings", "Restricted");
            overheatingScenario.OperatingAssumptions.Remove("Openings");

            Assert.Equal(guid, overheatingScenario.Key);
            Assert.Equal("MVRE", overheatingScenario.SystemTemplate?.Ventilation);
            Assert.Equal("Unrestricted", overheatingScenario.OperatingAssumptions.Value("Openings"));
        }

        /// <summary>
        /// The structural reason the test above holds rather than happening to: identity-defining state has
        /// no public setter, so there is no supported way to change what a scenario says after it is built.
        /// <para>
        /// <b>Written as an allow-list, not a check-list.</b> Naming the identity properties would mean a
        /// property added later with a public setter simply was not looked at - so instead every public
        /// property must be read-only unless it is one of the two that are explicitly presentation or
        /// provenance. A new settable property fails here, which is the point.
        /// </para>
        /// </summary>
        [Fact]
        public void IdentityDefiningState_HasNoPublicSetter()
        {
            string[] settable = ["Name", "Source"];

            foreach (PropertyInfo propertyInfo in typeof(OverheatingScenario).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (Array.IndexOf(settable, propertyInfo.Name) >= 0)
                {
                    continue;
                }

                Assert.True(propertyInfo.SetMethod == null || !propertyInfo.SetMethod.IsPublic, string.Format("OverheatingScenario.{0} is settable. Identity-defining state must not be, and if it is presentation or provenance it belongs in the allow-list with a reason.", propertyInfo.Name));
            }

            //And the two that are settable are genuinely not in the key - asserted, not assumed.
            foreach (string text in settable)
            {
                Assert.NotNull(typeof(OverheatingScenario).GetProperty(text));
            }
        }

        /// <summary>
        /// A scenario read from JSON written by a later version - an iteration this build has never heard
        /// of - is <c>Undefined</c> rather than an exception or, worse, the first member. An unreadable
        /// file helps nobody and a silently wrong mitigation stage is a wrong engineering answer.
        /// </summary>
        [Fact]
        public void UnknownEnumName_ReadsAsUndefined()
        {
            JsonObject jsonObject = Scenario().ToJsonObject();
            jsonObject["Iteration"] = "SomethingFromALaterVersion";

            OverheatingScenario overheatingScenario = new(jsonObject);

            Assert.Equal(PartOIteration.Undefined, overheatingScenario.Iteration);
        }

        /// <summary>
        /// <b>There is no foundation-stage iteration member.</b> The Iteration 0 work is a stage of this
        /// codebase, not an operating scenario of a building, and naming it in an engineering identity
        /// would outlive the schedule it describes. A scenario built during that work states
        /// <c>Undefined</c>, which is true.
        /// </summary>
        [Fact]
        public void PartOIteration_HasNoFoundationStageMember()
        {
            //The exact membership, not a substring rule: "no member contains a zero" would also forbid a
            //perfectly good name and would still miss "Foundation".
            //
            //BaseNaturalVentilation is LAST rather than beside BasePassive, and this test is where that
            //shows. Inserting it in reading order would renumber AcousticRestricted and ActiveTrimCooling,
            //which are persisted; the key derives from the NAME, so the ordinal carries no engineering
            //meaning and the two base configurations being far apart in this list means nothing about them.
            //DwellingIndependent (mixed dwelling strategies PR1) is appended for the same reason: it is the
            //iteration-neutral identity of an assessed common space, not a mitigation stage.
            Assert.Equal(["Undefined", "BasePassive", "AcousticRestricted", "ActiveTrimCooling", "BaseNaturalVentilation", "DwellingIndependent"], Enum.GetNames(typeof(PartOIteration)));
            Assert.Equal(["Undefined", "Dwelling", "CommonSpace"], Enum.GetNames(typeof(PartOAssessmentScope)));

            //Undefined is the default, so a scenario nobody filled in states no iteration rather than the
            //first real one.
            Assert.Equal(PartOIteration.Undefined, default(PartOIteration));
            Assert.Equal(PartOAssessmentScope.Undefined, default(PartOAssessmentScope));
        }

        /// <summary>
        /// A scenario that names nothing assessable says so. The iteration and the system may legitimately
        /// be unstated - a foundation-stage scenario over a real dwelling is valid.
        /// </summary>
        [Fact]
        public void IsValid_NeedsAScopeAndADesignZone()
        {
            Assert.True(Scenario().IsValid);
            Assert.True(new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Flat_1, PartOIteration.Undefined).IsValid);

            Assert.False(new OverheatingScenario().IsValid);
            Assert.False(new OverheatingScenario(PartOAssessmentScope.Undefined, guid_Flat_1, PartOIteration.BasePassive).IsValid);
            Assert.False(new OverheatingScenario(PartOAssessmentScope.Dwelling, Guid.Empty, PartOIteration.BasePassive).IsValid);
        }

        /// <summary>
        /// Equality is the key, so a scenario reconstructed from JSON is found in a set built before it was
        /// saved - which is how a result will be attributed to a scenario nobody kept a reference to.
        /// </summary>
        [Fact]
        public void Equality_IsTheDerivedKey()
        {
            OverheatingScenario overheatingScenario = Scenario();

            HashSet<OverheatingScenario> overheatingScenarios = [overheatingScenario];

            Assert.Contains(Core.Create.IJSAMObject<OverheatingScenario>(overheatingScenario.ToJsonObject().ToJsonString()), overheatingScenarios);
            Assert.DoesNotContain(Scenario(guid_Zone: guid_Flat_2), overheatingScenarios);
        }

        /// <summary>
        /// The copy constructor copies rather than shares: the copy is the same assessment, and changing
        /// the original's mutable parts afterwards does not move it.
        /// </summary>
        [Fact]
        public void CopyConstructor_CopiesTheWholeAssessment()
        {
            SystemTemplate systemTemplate = Template();
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = Assumptions();

            OverheatingScenario overheatingScenario = new(PartOAssessmentScope.Dwelling, guid_Flat_1, PartOIteration.BasePassive, systemTemplate, overheatingOperatingAssumptions)
            {
                Name = "Flat 1 - base",
                Source = "a model"
            };

            OverheatingScenario overheatingScenario_Copy = new(overheatingScenario);

            Assert.Equal(overheatingScenario.Key, overheatingScenario_Copy.Key);
            Assert.Equal(overheatingScenario.Name, overheatingScenario_Copy.Name);
            Assert.Equal(overheatingScenario.Source, overheatingScenario_Copy.Source);
            Assert.Equal(PartOIteration.BasePassive, overheatingScenario_Copy.Iteration);
            Assert.Equal("MVRE", overheatingScenario_Copy.VentilationStrategy);
            Assert.Equal("Unrestricted", overheatingScenario_Copy.OperatingAssumptions.Value("Openings"));

            //Nothing is shared with the original or with what built it.
            systemTemplate.Ventilation = "NV";
            overheatingOperatingAssumptions.Set("Openings", "Restricted");
            overheatingScenario.Name = "renamed";

            Assert.Equal(overheatingScenario.Key, overheatingScenario_Copy.Key);
            Assert.Equal("MVRE", overheatingScenario_Copy.VentilationStrategy);
            Assert.Equal("Flat 1 - base", overheatingScenario_Copy.Name);
        }

        /// <summary>
        /// <b><c>FromJsonObject</c> is the one public path that writes identity-defining state, so it is the
        /// one that could leave a scenario reporting the key it had before.</b> Loaded here over a populated
        /// instance, with JSON that omits every optional part - so a derivation that had held onto anything,
        /// or a read that merged instead of replacing, shows up as the old key or the old system.
        /// </summary>
        [Fact]
        public void FromJsonObject_ReplacesEverythingAndReKeys()
        {
            OverheatingScenario overheatingScenario = Scenario();
            overheatingScenario.Name = "Flat 1";
            overheatingScenario.Source = "a model";

            Guid guid = overheatingScenario.Key;

            Assert.True(overheatingScenario.FromJsonObject(new JsonObject
            {
                ["Scope"] = "CommonSpace",
                ["ZoneGuid"] = guid_Corridor.ToString("D"),
                ["Iteration"] = "Undefined"
            }));

            Assert.NotEqual(guid, overheatingScenario.Key);

            Assert.Equal(PartOAssessmentScope.CommonSpace, overheatingScenario.Scope);
            Assert.Equal(guid_Corridor, overheatingScenario.ZoneGuid);
            Assert.Null(overheatingScenario.Name);
            Assert.Null(overheatingScenario.Source);
            Assert.Null(overheatingScenario.SystemTemplate);
            Assert.False(overheatingScenario.HasVentilationStrategy);
            Assert.Equal(0, overheatingScenario.OperatingAssumptions.Count);

            //And it is exactly what the same statement built from scratch derives.
            Assert.Equal(new OverheatingScenario(PartOAssessmentScope.CommonSpace, guid_Corridor, PartOIteration.Undefined).Key, overheatingScenario.Key);
        }

        /// <summary>
        /// A scenario with nothing optional stated round-trips too - null system, null name, null
        /// provenance, no assumptions. The nulls are where a serialiser usually loses the distinction
        /// between "not stated" and "blank".
        /// </summary>
        [Fact]
        public void JsonRoundTrip_SurvivesTheNulls()
        {
            OverheatingScenario overheatingScenario = new(PartOAssessmentScope.CommonSpace, guid_Corridor, PartOIteration.Undefined);

            OverheatingScenario overheatingScenario_RoundTrip = Core.Create.IJSAMObject<OverheatingScenario>(overheatingScenario.ToJsonObject().ToJsonString());

            Assert.NotNull(overheatingScenario_RoundTrip);
            Assert.Equal(overheatingScenario.Key, overheatingScenario_RoundTrip.Key);
            Assert.Null(overheatingScenario_RoundTrip.Name);
            Assert.Null(overheatingScenario_RoundTrip.Source);
            Assert.Null(overheatingScenario_RoundTrip.SystemTemplate);
            Assert.Equal(0, overheatingScenario_RoundTrip.OperatingAssumptions.Count);
        }

        /// <summary>
        /// Malformed or partial JSON degrades rather than throwing - a scenario written by a later version,
        /// or hand-edited, must not make a whole model unreadable. And a mitigation stage this build does
        /// not have reads as <c>Undefined</c> rather than as a number cast into the enum: <c>Enum.TryParse</c>
        /// happily returns <c>(PartOIteration)99</c> for <c>"99"</c>, which would be a stage that does not
        /// exist reported as though it did.
        /// </summary>
        [Fact]
        public void MalformedJson_DegradesRatherThanThrowing()
        {
            OverheatingScenario overheatingScenario = new(new JsonObject
            {
                ["Scope"] = "Dwelling",
                ["ZoneGuid"] = "not a guid",
                ["Iteration"] = "99",
                ["Name"] = 42,
                ["OperatingAssumptions"] = new JsonObject { ["Assumptions"] = new JsonObject { ["SummerBypass"] = false } }
            });

            Assert.Equal(PartOAssessmentScope.Dwelling, overheatingScenario.Scope);
            Assert.Equal(Guid.Empty, overheatingScenario.ZoneGuid);
            Assert.Equal(PartOIteration.Undefined, overheatingScenario.Iteration);
            Assert.Null(overheatingScenario.Name);

            //Canonicalised through the same path Set(name, bool) uses, not taken at its JSON text - see
            //TypedBooleanAndJsonBoolean_DeriveSameKey.
            Assert.Equal("False", overheatingScenario.OperatingAssumptions.Value("SummerBypass"));

            //An object that is not a scenario at all is refused rather than reported as an empty one.
            OverheatingScenario overheatingScenario_Other = new();

            Assert.False(overheatingScenario_Other.FromJsonObject(new JsonObject { ["Something"] = "else" }));
            Assert.False(overheatingScenario_Other.FromJsonObject(null));
        }

        /// <summary>
        /// A scenario that names nothing assessable has no identity, so its key is empty rather than a
        /// real-looking guid every other half-filled scenario would share.
        /// </summary>
        [Fact]
        public void InvalidScenario_HasNoKey()
        {
            Assert.Equal(Guid.Empty, new OverheatingScenario().Key);
            Assert.Equal(Guid.Empty, new OverheatingScenario(PartOAssessmentScope.Dwelling, Guid.Empty, PartOIteration.BasePassive, Template(), Assumptions()).Key);

            Assert.NotEqual(Guid.Empty, Scenario().Key);
        }

        /// <summary>
        /// <b>And having no identity is not an identity they all share.</b> Several half-filled scenarios -
        /// which is what a user interface holds while somebody is still choosing dwellings - must not
        /// collapse into one entry of a set just because none of them has said what it is yet. Equality
        /// falls back to reference for those, and <c>GetHashCode</c> with it.
        /// </summary>
        [Fact]
        public void InvalidScenarios_DoNotCollapseIntoOne()
        {
            OverheatingScenario overheatingScenario_1 = new();
            OverheatingScenario overheatingScenario_2 = new();

            Assert.NotEqual(overheatingScenario_1, overheatingScenario_2);
            Assert.Equal(overheatingScenario_1, overheatingScenario_1);

            HashSet<OverheatingScenario> overheatingScenarios = [overheatingScenario_1, overheatingScenario_2, new OverheatingScenario()];

            Assert.Equal(3, overheatingScenarios.Count);
            Assert.Contains(overheatingScenario_1, overheatingScenarios);

            //An incomplete scenario is not equal to a complete one either, whichever way round it is asked.
            Assert.NotEqual((object)overheatingScenario_1, Scenario());
            Assert.NotEqual((object)Scenario(), overheatingScenario_1);
        }

        /// <summary>
        /// Equality is total: not equal to null, not equal to something that is not a scenario, and
        /// <c>GetHashCode</c> agrees with it across a save and reload - which is what a <c>HashSet</c>
        /// lookup actually depends on.
        /// </summary>
        [Fact]
        public void Equality_IsTotal()
        {
            OverheatingScenario overheatingScenario = Scenario();

            Assert.False(overheatingScenario.Equals(null));
            Assert.False(overheatingScenario.Equals("not a scenario"));
            Assert.True(overheatingScenario.Equals(new OverheatingScenario(overheatingScenario)));

            Assert.Equal(overheatingScenario.GetHashCode(), Core.Create.IJSAMObject<OverheatingScenario>(overheatingScenario.ToJsonObject().ToJsonString()).GetHashCode());
        }

        /// <summary>
        /// The ventilation strategy is readable off the scenario, and is the existing <c>MVRE</c>/<c>NV</c>
        /// vocabulary rather than a second one. <see cref="OverheatingScenario.HasVentilationStrategy"/> is
        /// separate from <c>IsValid</c> because a scenario can name a dwelling without naming a system, and
        /// the consumer that makes the scenario authoritative over the strategy has to refuse in that case
        /// rather than fall back to the zone-name lookup it is replacing.
        /// </summary>
        [Fact]
        public void VentilationStrategy_IsReadableAndIsNotASecondVocabulary()
        {
            Assert.Equal("MVRE", Scenario().VentilationStrategy);
            Assert.True(Scenario().HasVentilationStrategy);

            Assert.Equal("NV", Scenario(systemTemplate: Template("NV")).VentilationStrategy);

            OverheatingScenario overheatingScenario = new(PartOAssessmentScope.Dwelling, guid_Flat_1, PartOIteration.Undefined);

            Assert.True(overheatingScenario.IsValid);
            Assert.False(overheatingScenario.HasVentilationStrategy);
            Assert.Null(overheatingScenario.VentilationStrategy);
        }

        /// <summary>
        /// <c>OverheatingOperatingAssumptions</c> in its own right: what it stores, what it refuses, and
        /// that it round-trips on its own.
        /// </summary>
        [Fact]
        public void OperatingAssumptions_BehaveAsDocumented()
        {
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = new();

            //A blank name is dropped - an assumption nobody can read back would only make the key depend on
            //something invisible.
            overheatingOperatingAssumptions.Set(null, "x");
            overheatingOperatingAssumptions.Set("   ", "x");
            Assert.Equal(0, overheatingOperatingAssumptions.Count);

            //A null value is stated as blank, which is not the same as unstated.
            overheatingOperatingAssumptions.Set("Openings", null);
            Assert.Equal(string.Empty, overheatingOperatingAssumptions.Value("Openings"));
            Assert.True(overheatingOperatingAssumptions.Contains("Openings"));
            Assert.Null(overheatingOperatingAssumptions.Value("Nothing"));

            overheatingOperatingAssumptions.Set("Boost", true);
            overheatingOperatingAssumptions.Set("Alpha", 1.0);

            //Canonical ordinal order, whatever order they were stated in.
            Assert.Equal(["Alpha", "Boost", "Openings"], overheatingOperatingAssumptions.Names);
            Assert.Equal(3, overheatingOperatingAssumptions.Count);

            Assert.True(overheatingOperatingAssumptions.Remove("Boost"));
            Assert.False(overheatingOperatingAssumptions.Remove("Boost"));
            Assert.Equal(2, overheatingOperatingAssumptions.Count);

            //The returned list is a copy.
            overheatingOperatingAssumptions.ToList().Clear();
            Assert.Equal(2, overheatingOperatingAssumptions.Count);

            OverheatingOperatingAssumptions overheatingOperatingAssumptions_RoundTrip = Core.Create.IJSAMObject<OverheatingOperatingAssumptions>(overheatingOperatingAssumptions.ToJsonObject().ToJsonString());

            Assert.NotNull(overheatingOperatingAssumptions_RoundTrip);
            Assert.Equal(overheatingOperatingAssumptions.Names, overheatingOperatingAssumptions_RoundTrip.Names);
            Assert.Equal(string.Empty, overheatingOperatingAssumptions_RoundTrip.Value("Openings"));
            Assert.Equal("1", overheatingOperatingAssumptions_RoundTrip.Value("Alpha"));
        }

        // ---------------------------------------------------------------------------------------------
        // Fixture
        // ---------------------------------------------------------------------------------------------

        /// <summary>Flat 1 of the validation block, on MVRE, with a stated set of assumptions.</summary>
        private static OverheatingScenario Scenario(PartOAssessmentScope partOAssessmentScope = PartOAssessmentScope.Dwelling, Guid? guid_Zone = null, PartOIteration partOIteration = PartOIteration.Undefined, SystemTemplate systemTemplate = null, OverheatingOperatingAssumptions overheatingOperatingAssumptions = null)
        {
            return new OverheatingScenario(partOAssessmentScope, guid_Zone ?? guid_Flat_1, partOIteration, systemTemplate ?? Template(), overheatingOperatingAssumptions ?? Assumptions());
        }

        /// <summary>MVRE - SAM's heat-recovery ventilation - with radiators and no cooling.</summary>
        private static SystemTemplate Template(string ventilation = "MVRE", string heating = "RAD1", string cooling = "UC1", string plantRoom = "PR1", string controls = "CTL1", string version = "1")
        {
            return new SystemTemplate(ventilation, heating, cooling, plantRoom, controls, version);
        }

        private static OverheatingOperatingAssumptions Assumptions()
        {
            OverheatingOperatingAssumptions result = new();

            result.Set("Openings", "Unrestricted");
            result.Set("MechanicalRate_Lps", 21.0);
            result.Set("SummerBypass", false);

            return result;
        }
    }
}
