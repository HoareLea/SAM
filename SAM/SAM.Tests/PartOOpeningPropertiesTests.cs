// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Aperture-level opening restriction, independent of the Part O scenario/iteration vocabulary.</b>
    /// <para>
    /// <c>PartOOpeningProperties.OpeningRestriction</c> states whether, and when, an opening may be used for
    /// overheating ventilation - a closed <c>Unrestricted</c>/<c>NightClosed</c>/<c>AlwaysClosed</c> set
    /// rather than independent booleans, so an opening cannot state a contradictory combination. These
    /// tests cover the domain model in isolation: JSON compatibility with data serialised before this
    /// member existed, copy/clone semantics, the derived availability <c>DailyAvailabilitySchedule</c>, and
    /// <c>Modify.ResetPartOOpeningRestrictions</c>, which is what lets a BasePassive ("openings operated
    /// without restriction") iteration enforce its own assumption on a copy of the model.
    /// </para>
    /// <para>
    /// What this does NOT cover: the TAS-side write (schedule creation/reuse on a real TBD aperture type),
    /// which needs the TAS COM interop and is exercised by SAM_Tas's own TM59 test project instead.
    /// </para>
    /// </summary>
    public class PartOOpeningPropertiesTests
    {
        // -------------------------------------------------------------------------------------------------
        // Defaults and legacy compatibility
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void Default_IsUnrestricted_WithNoSchedule()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.0, 1.0, 30.0);

            Assert.Equal(OpeningRestriction.Unrestricted, partOOpeningProperties.OpeningRestriction);
            Assert.Null(partOOpeningProperties.Schedule);
        }

        /// <summary>
        /// A <c>PartOOpeningProperties</c> serialised before <c>OpeningRestriction</c> existed carries no
        /// such key. It must deserialise to <c>Unrestricted</c> - the legacy behaviour - not to some other
        /// state, and it must not throw or drop the fields it does carry.
        /// </summary>
        [Fact]
        public void LegacyJson_WithNoOpeningRestrictionKey_DeserialisesAsUnrestricted()
        {
            string legacyJson = @"{
                ""_type"": ""SAM.Analytical.PartOOpeningProperties"",
                ""Width"": 1.2,
                ""Height"": 1.0,
                ""OpeningAngle"": 30.0,
                ""Factor"": 1.0
            }";

            PartOOpeningProperties partOOpeningProperties = SAM.Core.Create.IJSAMObject<PartOOpeningProperties>(legacyJson);

            Assert.NotNull(partOOpeningProperties);
            Assert.Equal(OpeningRestriction.Unrestricted, partOOpeningProperties.OpeningRestriction);
            Assert.Null(partOOpeningProperties.Schedule);
            Assert.Equal(1.2, partOOpeningProperties.Width);
            Assert.Equal(1.0, partOOpeningProperties.Height);
        }

        [Fact]
        public void JsonRoundTrip_PreservesNightClosedAndHours()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 7, 22);

            PartOOpeningProperties reconstructed = RoundTrip.Once(partOOpeningProperties);

            Assert.Equal(OpeningRestriction.NightClosed, reconstructed.OpeningRestriction);
            Assert.Equal(7, reconstructed.NightOpenFromHour);
            Assert.Equal(22, reconstructed.NightOpenToHour);
        }

        /// <summary>
        /// A PRESENT but unrecognised restriction name is corrupt or newer file data. Reading it silently as
        /// <c>Unrestricted</c> would re-open an opening recorded as restricted, so deserialization refuses -
        /// the same contract <c>DailyAvailabilitySchedule</c> applies to its own malformed file data. Only
        /// the ABSENT key is legacy and means <c>Unrestricted</c>.
        /// </summary>
        [Fact]
        public void Json_WithUnrecognisedOpeningRestriction_RefusesDeserialisation()
        {
            PartOOpeningProperties partOOpeningProperties = new(1.2, 1.0, 30.0, OpeningRestriction.NightClosed);

            System.Text.Json.Nodes.JsonObject jsonObject = System.Text.Json.Nodes.JsonObject.Parse(@"{ ""OpeningRestriction"": ""NightClosedd"" }") as System.Text.Json.Nodes.JsonObject;

            Assert.False(partOOpeningProperties.FromJsonObject(jsonObject), "An unrecognised restriction name deserialised, which would silently read as Unrestricted.");
        }

        /// <summary>
        /// The Grasshopper parse path accepts lowercase text through <c>Core.Query.TryGetEnum</c>'s
        /// case-insensitive fallback. That comparison must be culture-INVARIANT: under a Turkish culture the
        /// culture-sensitive <c>ToUpper()</c> maps "nightclosed" to "NİGHTCLOSED" (dotless capital I) and
        /// rejects a perfectly valid input, silently substituting <c>Unrestricted</c>.
        /// </summary>
        [Fact]
        public void OpeningRestriction_LowercaseParse_IsCultureInvariant()
        {
            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");

                Assert.True(Core.Query.TryGetEnum<OpeningRestriction>("nightclosed", out OpeningRestriction openingRestriction));
                Assert.Equal(OpeningRestriction.NightClosed, openingRestriction);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
            }
        }

        /// <summary>
        /// The availability schedule is DERIVED, never persisted - so a round trip must reproduce it from
        /// the restriction and the hours rather than carrying 24 values through the JSON.
        /// </summary>
        [Fact]
        public void JsonRoundTrip_RederivesTheSameScheduleValues()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 7, 22);

            PartOOpeningProperties reconstructed = RoundTrip.Once(partOOpeningProperties);

            Assert.Equal(partOOpeningProperties.Schedule.Name, reconstructed.Schedule.Name);
            Assert.True(partOOpeningProperties.Schedule.ValuesEqual(reconstructed.Schedule));
        }

        [Fact]
        public void JsonRoundTrip_PreservesAlwaysClosed()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.AlwaysClosed);

            PartOOpeningProperties reconstructed = RoundTrip.Once(partOOpeningProperties);

            Assert.Equal(OpeningRestriction.AlwaysClosed, reconstructed.OpeningRestriction);
            Assert.Null(reconstructed.Schedule);
        }

        // -------------------------------------------------------------------------------------------------
        // Copy constructor
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void CopyConstructor_PreservesOpeningRestrictionAndHours()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 9, 20);

            PartOOpeningProperties copy = new PartOOpeningProperties(partOOpeningProperties);

            Assert.Equal(OpeningRestriction.NightClosed, copy.OpeningRestriction);
            Assert.Equal(9, copy.NightOpenFromHour);
            Assert.Equal(20, copy.NightOpenToHour);
            Assert.True(partOOpeningProperties.Schedule.ValuesEqual(copy.Schedule));
        }

        // -------------------------------------------------------------------------------------------------
        // Derived availability schedule
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The exact expectation the real TAS acceptance run checks: hours 08-22 available, 23:00-24:00 not,
        /// under the name looked for in the TBD Schedule database.
        /// </summary>
        [Fact]
        public void NightClosed_DefaultWindow_ProducesDeterministicallyNamed24HourSchedule()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed);

            DailyAvailabilitySchedule schedule = partOOpeningProperties.Schedule;

            Assert.NotNull(schedule);
            Assert.Equal("PartO_DayOpen_08_23", schedule.Name);
            Assert.Equal("000000001111111111111110", schedule.ValuesText);
            Assert.Equal("00FFFE", schedule.Signature);

            bool[] values = schedule.GetValues();
            Assert.Equal(24, values.Length);

            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal(hour >= 8 && hour < 23, values[hour]);
            }
        }

        [Fact]
        public void NightClosed_CustomWindow_NamesAndValuesMatchTheWindow()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 6, 21);

            DailyAvailabilitySchedule schedule = partOOpeningProperties.Schedule;

            Assert.NotNull(schedule);
            Assert.Equal("PartO_DayOpen_06_21", schedule.Name);

            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal(hour >= 6 && hour < 21, schedule[hour]);
            }
        }

        /// <summary>
        /// Grasshopper's openingHour_/closingHour_ allow closingHour_ &lt; openingHour_ (e.g. available
        /// 22:00-06:00) - the window must wrap across midnight rather than producing an inverted/empty range.
        /// </summary>
        [Fact]
        public void NightClosed_OvernightWindow_WrapsAcrossMidnight()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 22, 6);

            DailyAvailabilitySchedule schedule = partOOpeningProperties.Schedule;

            Assert.NotNull(schedule);
            Assert.Equal("PartO_DayOpen_22_06", schedule.Name);

            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal(hour >= 22 || hour < 6, schedule[hour]);
            }
        }

        /// <summary>
        /// An equal opening/closing hour is not refused by the domain object - it deterministically produces
        /// an always-unavailable (all-zero) schedule. Grasshopper flags this combination with a warning rather
        /// than silently accepting it, but the underlying domain behaviour this pins is what that warning
        /// describes - unchanged by the move from Profile to DailyAvailabilitySchedule.
        /// </summary>
        [Fact]
        public void NightClosed_EqualOpeningAndClosingHour_ProducesAlwaysUnavailableSchedule()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 8, 8);

            DailyAvailabilitySchedule schedule = partOOpeningProperties.Schedule;

            Assert.All(schedule.GetValues(), value => Assert.False(value));
            Assert.Equal("000000", schedule.Signature);
        }

        /// <summary>
        /// Hours outside 0-23 (as could arrive from a Grasshopper Param_Integer wired to an arbitrary
        /// integer) are normalised modulo 24 rather than throwing or producing an out-of-range window.
        /// </summary>
        [Fact]
        public void NightClosed_OutOfRangeHours_AreNormalisedModulo24()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 32, -1);

            DailyAvailabilitySchedule schedule = partOOpeningProperties.Schedule;

            Assert.Equal("PartO_DayOpen_08_23", schedule.Name);

            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal(hour >= 8 && hour < 23, schedule[hour]);
            }
        }

        [Fact]
        public void TwoNightClosedInstances_WithTheSameWindow_ProduceTheSameScheduleNameAndValues()
        {
            //Reuse across apertures depends on the VALUES matching - that is what the TAS-side writer
            //compares. The name matching too is what keeps a newly created schedule's name stable.
            PartOOpeningProperties a = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed);
            PartOOpeningProperties b = new PartOOpeningProperties(0.6, 1.4, 90.0, OpeningRestriction.NightClosed);

            Assert.Equal(a.Schedule.Name, b.Schedule.Name);
            Assert.Equal(a.Schedule.Signature, b.Schedule.Signature);
            Assert.True(a.Schedule.ValuesEqual(b.Schedule));
        }

        /// <summary>
        /// A different window is a different schedule by value, so the TAS side must not reuse one for the
        /// other however similar the names look.
        /// </summary>
        [Fact]
        public void NightClosed_DifferentWindows_AreNotValueEqual()
        {
            PartOOpeningProperties a = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 8, 23);
            PartOOpeningProperties b = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed, 8, 22);

            Assert.False(a.Schedule.ValuesEqual(b.Schedule));
            Assert.NotEqual(a.Schedule.Signature, b.Schedule.Signature);
        }

        /// <summary>
        /// AlwaysClosed carries no availability schedule at all: the TAS transfer expresses it as an opening
        /// factor of 0, not as a second, all-zero schedule.
        /// </summary>
        [Fact]
        public void AlwaysClosed_HasNoSchedule()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.AlwaysClosed);

            Assert.Null(partOOpeningProperties.Schedule);
        }

        [Fact]
        public void Unrestricted_HasNoSchedule()
        {
            PartOOpeningProperties partOOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.Unrestricted);

            Assert.Null(partOOpeningProperties.Schedule);
        }

        // -------------------------------------------------------------------------------------------------
        // Modify.ResetPartOOpeningRestrictions
        // -------------------------------------------------------------------------------------------------

        private static readonly Construction wallConstruction = new Construction(Guid.NewGuid(), "Wall");
        private static readonly ApertureConstruction windowConstruction = new ApertureConstruction(Guid.NewGuid(), "Window", ApertureType.Window);

        private static Point3D P(double x, double y, double z) => new Point3D(x, y, z);

        private static Face3D WallFace() => new Face3D(new Polygon3D(new[] { P(0, 0, 0), P(10, 0, 0), P(10, 10, 0), P(0, 10, 0) }));
        private static Face3D ApertureFace() => new Face3D(new Polygon3D(new[] { P(1, 1, 0), P(3, 1, 0), P(3, 3, 0), P(1, 3, 0) }));

        [Fact]
        public void ResetPartOOpeningRestrictions_NightClosedAperture_BecomesUnrestricted()
        {
            Panel panel = AnalyticalCreate.Panel(wallConstruction, PanelType.Wall, WallFace());
            Aperture aperture = AnalyticalCreate.Aperture(windowConstruction, ApertureFace());
            aperture.AddSingleOpeningProperties(new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed));
            panel.AddAperture(aperture);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(panel);

            AnalyticalModel analyticalModel = new AnalyticalModel("Reset Test", null, null, null, adjacencyCluster);

            AnalyticalModel analyticalModel_Reset = analyticalModel.ResetPartOOpeningRestrictions(out List<string> notes);

            Assert.NotEmpty(notes);

            Aperture aperture_Reset = analyticalModel_Reset.AdjacencyCluster.GetApertures().Find(x => x.Guid == aperture.Guid);
            Assert.True(aperture_Reset.TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties_Reset));
            Assert.IsType<PartOOpeningProperties>(openingProperties_Reset);
            Assert.Equal(OpeningRestriction.Unrestricted, ((PartOOpeningProperties)openingProperties_Reset).OpeningRestriction);
        }

        /// <summary>Invariant §2/11: preparation works on a copy, and the original is never mutated.</summary>
        [Fact]
        public void ResetPartOOpeningRestrictions_OriginalModelIsUnchanged()
        {
            Panel panel = AnalyticalCreate.Panel(wallConstruction, PanelType.Wall, WallFace());
            Aperture aperture = AnalyticalCreate.Aperture(windowConstruction, ApertureFace());
            aperture.AddSingleOpeningProperties(new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed));
            panel.AddAperture(aperture);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(panel);

            AnalyticalModel analyticalModel = new AnalyticalModel("Reset Test", null, null, null, adjacencyCluster);

            string json_Before = SAM.Core.Convert.ToString(analyticalModel);

            analyticalModel.ResetPartOOpeningRestrictions(out List<string> _);

            string json_After = SAM.Core.Convert.ToString(analyticalModel);

            Assert.True(JsonEquivalence.AreEquivalent(json_Before, json_After, out string difference), difference);
        }

        /// <summary>Non-Part-O opening data (here, a plain OpeningProperties) is never this method's to change.</summary>
        [Fact]
        public void ResetPartOOpeningRestrictions_NonPartOOpeningProperties_IsLeftUntouched()
        {
            Panel panel = AnalyticalCreate.Panel(wallConstruction, PanelType.Wall, WallFace());
            Aperture aperture = AnalyticalCreate.Aperture(windowConstruction, ApertureFace());

            OpeningProperties openingProperties = new OpeningProperties(0.6) { Factor = 0.5 };
            aperture.AddSingleOpeningProperties(openingProperties);
            panel.AddAperture(aperture);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(panel);

            AnalyticalModel analyticalModel = new AnalyticalModel("Reset Test", null, null, null, adjacencyCluster);

            AnalyticalModel analyticalModel_Reset = analyticalModel.ResetPartOOpeningRestrictions(out List<string> notes);

            Assert.Empty(notes);

            Aperture aperture_Reset = analyticalModel_Reset.AdjacencyCluster.GetApertures().Find(x => x.Guid == aperture.Guid);
            Assert.True(aperture_Reset.TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties_Reset));
            Assert.IsType<OpeningProperties>(openingProperties_Reset);
            Assert.Equal(0.5, ((OpeningProperties)openingProperties_Reset).Factor);
        }

        [Fact]
        public void ResetPartOOpeningRestrictions_AlreadyUnrestricted_ReturnsSameModelUnchanged()
        {
            Panel panel = AnalyticalCreate.Panel(wallConstruction, PanelType.Wall, WallFace());
            Aperture aperture = AnalyticalCreate.Aperture(windowConstruction, ApertureFace());
            aperture.AddSingleOpeningProperties(new PartOOpeningProperties(1.2, 1.0, 30.0));
            panel.AddAperture(aperture);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(panel);

            AnalyticalModel analyticalModel = new AnalyticalModel("Reset Test", null, null, null, adjacencyCluster);

            AnalyticalModel analyticalModel_Reset = analyticalModel.ResetPartOOpeningRestrictions(out List<string> notes);

            Assert.Empty(notes);
            Assert.Same(analyticalModel, analyticalModel_Reset);
        }
    }
}
