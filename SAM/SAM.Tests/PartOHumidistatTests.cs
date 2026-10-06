// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using System;
using System.Collections.Generic;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>The humidistat on an air handling unit's generated TAS plant zone, and the Check rule that now
    /// reports an invalid one before TAS does.</b>
    ///
    /// <para><b>The defect</b></para>
    /// <para>
    /// <c>Modify.AddAirMovementObjects</c> builds one <see cref="AirHandlingUnitAirMovement"/> per air
    /// handling unit, carrying the supply condition the unit's own TAS zone is given.
    /// <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> writes that unit a small TAS zone named after it
    /// ("MVHR-01") and writes these profiles onto its internal condition: Heating to the temperature LOWER
    /// limit, Cooling to the UPPER, <b>Humidification to the humidity LOWER limit</b> (TBD
    /// <c>Profiles.ticHLL</c>) and <b>Dehumidification to the humidity UPPER limit</b> (<c>ticHUL</c>).
    /// </para>
    /// <para>
    /// The two humidity values were transposed - Humidification 100%, Dehumidification 0% - so the
    /// generated internal condition asked TAS to hold the unit's air above 100% and below 0% relative
    /// humidity at once. TAS's own pre-simulation check refused the model outright: <i>"Internal Condition
    /// 'MVHR-01' humidistat has overlapping limits."</i> Read out of the licensed TBD that failed:
    /// <c>ticHLL</c> value 100 setback 0, <c>ticHUL</c> value 0 setback 100 - TAS's own correct defaults
    /// still in the setbacks beside them, which is what a transposition of the VALUES alone looks like.
    /// </para>
    ///
    /// <para><b>It predates the isolation work, and Part O</b></para>
    /// <para>
    /// The transposed pair has been in <c>AddAirMovementObjects</c> since January 2024, and that is a
    /// general SAM path: every model with an air handling unit gets these profiles, whether or not Part O,
    /// an iteration or an isolated run is involved. Isolated testing did not introduce it -
    /// <see cref="TheTransposedPair_WasNeverPartOSpecific_ItIsTheGeneralAirHandlingUnitPath"/> pins that
    /// the source is the general path.
    /// </para>
    ///
    /// <para><b>What "no humidity control" is</b></para>
    /// <para>
    /// An MVHR neither humidifies nor dehumidifies, and the SAM convention for saying so is a pair of
    /// limits that can never be reached - lower 0%, upper 100% - not an absent profile. That is verbatim
    /// the pair the shipped profile library states as "No Humidification" (0) and "No Dehumidification"
    /// (100), and also the pair TAS's own new-internal-condition defaults carry. No heating, cooling,
    /// humidification or dehumidification control is invented to satisfy TAS: these limits are inert.
    /// </para>
    ///
    /// <para><b>And HDD/CDD stay out of it</b></para>
    /// <para>
    /// TAS also warns that the generated zone "is missing internal conditions on some daytypes". That is
    /// intentional - <c>UpdateIZAMs</c> excludes the HDD and CDD design daytypes from the plant zone's
    /// internal condition on purpose - and nothing here adds them. It is a warning about a deliberate
    /// state, so it is not and must not become a reason not to simulate. Pinned in SAM_Tas at
    /// <c>Query.DayType_PlantZoneInternalCondition</c>; the prepared-model regressions are in
    /// <c>PartOIterationPreparationTests</c>.
    /// </para>
    /// </summary>
    public class PartOHumidistatTests
    {
        // ---- A. The rule, on the object the plant zone is generated from -------------------------------

        /// <summary>
        /// An air movement whose humidity limits are the transposed pair the defect wrote is an
        /// <see cref="LogRecordType.Error"/>, naming the object and its Guid.
        /// </summary>
        [Fact]
        public void AirMovement_WithTransposedHumidityLimits_IsAnError()
        {
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = Movement("MVHR-01", 100, 0);

            LogRecord logRecord = Assert.Single(AnalyticalCreate.Log(airHandlingUnitAirMovement));

            Assert.Equal(LogRecordType.Error, logRecord.LogRecordType);
            Assert.Contains("MVHR-01", logRecord.Text);
            Assert.Contains(airHandlingUnitAirMovement.Guid.ToString(), logRecord.Text);
            Assert.Contains("overlapping humidity limits", logRecord.Text);
        }

        /// <summary>The corrected pair - no humidity control - reports nothing at all, not even a warning.</summary>
        [Fact]
        public void AirMovement_WithNoHumidityControl_ReportsNothing()
        {
            Assert.Empty(AnalyticalCreate.Log(Movement("MVHR-01", 0, 100)));
        }

        /// <summary>
        /// Limits that are equal do not overlap. Holding one exact humidity is a real, if unusual, control
        /// state and TAS accepts it, so the rule must not report it.
        /// </summary>
        [Fact]
        public void AirMovement_WithEqualHumidityLimits_ReportsNothing()
        {
            Assert.Empty(AnalyticalCreate.Log(Movement("MVHR-01", 50, 50)));
        }

        /// <summary>
        /// One limit alone is never an overlap. It takes a pair to be wrong, and an absent lower limit must
        /// not be read as "having no humidity control is an error".
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void AirMovement_WithAMissingHumidityLimit_ReportsNothing(bool omit_LowerLimit, bool omit_UpperLimit)
        {
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(
                "MVHR-01",
                null,
                null,
                omit_LowerLimit ? null : Humidity(ProfileType.Humidification, 100),
                omit_UpperLimit ? null : Humidity(ProfileType.Dehumidification, 0),
                null);

            Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement));
        }

        /// <summary>
        /// <b>Two schedules that pass each other are not an overlap.</b> The lower limit's highest value
        /// (40%) is above the upper limit's lowest (20%), but never at the same hour - so a rule that
        /// compared one profile's maximum against the other's minimum would report this valid pair as an
        /// error. The rule reads index by index precisely so that it cannot.
        /// </summary>
        [Fact]
        public void AirMovement_WithSchedulesThatNeverCross_ReportsNothing()
        {
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(
                "AHU Scheduled",
                null,
                null,
                new Profile("Lower Limit", ProfileType.Humidification, Hourly(40, 10)),
                new Profile("Upper Limit", ProfileType.Dehumidification, Hourly(50, 20)),
                null);

            Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement));
        }

        /// <summary>
        /// A pair that does cross - at the afternoon hours only - is reported, and the record says at which
        /// hour, so the schedule can be found.
        /// </summary>
        [Fact]
        public void AirMovement_WithSchedulesThatCrossInTheAfternoon_IsAnErrorNamingThatHour()
        {
            AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(
                "AHU Scheduled",
                null,
                null,
                new Profile("Lower Limit", ProfileType.Humidification, Hourly(40, 60)),
                new Profile("Upper Limit", ProfileType.Dehumidification, Hourly(50, 50)),
                null);

            LogRecord logRecord = Assert.Single(AnalyticalCreate.Log(airHandlingUnitAirMovement));

            Assert.Equal(LogRecordType.Error, logRecord.LogRecordType);

            //Hour 12 is the first hour of the afternoon block, and so the first crossing there is.
            Assert.Contains("at hour 12", logRecord.Text);
        }

        // ---- B. The same rule on an ordinary room's internal condition ---------------------------------

        /// <summary>
        /// The state TAS refuses is a general invalid <see cref="InternalCondition"/>, not a Part O one, so
        /// the rule is on the internal condition too: a room whose humidification setpoint is above its
        /// dehumidification setpoint is an error wherever it came from.
        /// </summary>
        [Fact]
        public void InternalCondition_WithOverlappingHumidityLimits_IsAnError()
        {
            InternalCondition internalCondition = InternalCondition_Humidity(100, 0, out ProfileLibrary profileLibrary);

            LogRecord logRecord = Assert.Single(HumidityRecords(AnalyticalCreate.Log(internalCondition, profileLibrary)));

            Assert.Equal(LogRecordType.Error, logRecord.LogRecordType);
            Assert.Contains("Office", logRecord.Text);
            Assert.Contains(internalCondition.Guid.ToString(), logRecord.Text);
            Assert.Contains("overlapping humidistat limits", logRecord.Text);
        }

        /// <summary>
        /// And the shipped "No Humidification" (0) / "No Dehumidification" (100) pair - the convention this
        /// correction restores - reports nothing.
        /// </summary>
        [Fact]
        public void InternalCondition_WithTheShippedNoHumidityControlPair_ReportsNothing()
        {
            InternalCondition internalCondition = InternalCondition_Humidity(0, 100, out ProfileLibrary profileLibrary);

            Assert.Empty(HumidityRecords(AnalyticalCreate.Log(internalCondition, profileLibrary)));
        }

        // ---- C. Where the rule runs from ---------------------------------------------------------------

        /// <summary>
        /// <b>The cluster-level Check reaches it.</b> An air handling unit's air movement is related to the
        /// unit, not to any space, so nothing that walks the spaces can find it - and the space and panel
        /// checks give up early on a cluster that has neither. The rule therefore runs before them, and a
        /// cluster carrying nothing but the offending plant object still reports it.
        /// </summary>
        [Fact]
        public void AdjacencyClusterCheck_ReportsTheAirMovement_EvenWithNoSpacesOrPanels()
        {
            AdjacencyCluster adjacencyCluster = new();
            adjacencyCluster.AddObject(Movement("MVHR-01", 100, 0));

            Log log = AnalyticalCreate.Log(adjacencyCluster);

            LogRecord logRecord = Assert.Single(HumidityRecords(log));

            Assert.Equal(LogRecordType.Error, logRecord.LogRecordType);
            Assert.Contains("MVHR-01", logRecord.Text);
        }

        /// <summary>
        /// <b>The defect was never Part O's and never isolation's.</b>
        /// <c>Modify.AddAirMovementObjects</c> is the general path every model with an air handling unit
        /// goes through, and the transposed pair was written there. Called with no Part O, no iteration and
        /// no isolation anywhere near it, it states the valid pair.
        /// </summary>
        [Fact]
        public void TheTransposedPair_WasNeverPartOSpecific_ItIsTheGeneralAirHandlingUnitPath()
        {
            //The least a model needs for the general path to build a unit's air movement: one space, one
            //ventilation system serving it, and the unit that system names. No Part F data, no dwelling
            //zone, no iteration, no isolation.
            AdjacencyCluster adjacencyCluster = new();

            Space space = new("Office");
            adjacencyCluster.AddObject(space);

            VentilationSystem ventilationSystem = new("AHU-99", new VentilationSystemType("MVHR", "Fixture"));
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, "AHU-99");
            adjacencyCluster.AddObject(ventilationSystem);
            adjacencyCluster.AddRelation(ventilationSystem, space);

            adjacencyCluster.AddObject(new AirHandlingUnit("AHU-99", 20, 20));

            adjacencyCluster.AddAirMovementObjects(null);

            AirHandlingUnitAirMovement airHandlingUnitAirMovement = adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>()?.Find(x => x.Name == "AHU-99");

            Assert.NotNull(airHandlingUnitAirMovement);

            //The humidity LOWER limit is 0% and the UPPER limit is 100%, not the other way round.
            Assert.Equal(0, airHandlingUnitAirMovement.Humidification[0], 6);
            Assert.Equal(100, airHandlingUnitAirMovement.Dehumidification[0], 6);

            Assert.Empty(AnalyticalCreate.Log(airHandlingUnitAirMovement));
        }

        /// <summary>
        /// <b>Checking does not change the model.</b> The pre-simulation gate runs over the model that is
        /// about to be simulated, so a Check that modified anything would be changing the simulated design.
        /// </summary>
        [Fact]
        public void Check_DoesNotModifyTheCluster()
        {
            AdjacencyCluster adjacencyCluster = new();
            adjacencyCluster.AddObject(Movement("MVHR-01", 100, 0));

            string before = adjacencyCluster.ToJsonObject().ToJsonString();

            AnalyticalCreate.Log(adjacencyCluster);

            Assert.Equal(before, adjacencyCluster.ToJsonObject().ToJsonString());
        }

        /// <summary>
        /// <b>The rule never writes a warning, so it can never promote one.</b> It contributes exactly one
        /// record - an Error - when the pair overlaps and nothing when it does not, which is what keeps the
        /// intentional HDD/CDD warning a warning.
        /// </summary>
        [Theory]
        [InlineData(0, 100)]
        [InlineData(50, 50)]
        [InlineData(100, 0)]
        public void TheRule_WritesErrorsOnly_NeverWarnings(double lowerLimit, double upperLimit)
        {
            foreach (LogRecord logRecord in AnalyticalCreate.Log(Movement("MVHR-01", lowerLimit, upperLimit)))
            {
                Assert.Equal(LogRecordType.Error, logRecord.LogRecordType);
            }
        }

        // ---- The fixture -------------------------------------------------------------------------------

        /// <summary>An air movement stating one humidity lower limit and one upper limit, and nothing else.</summary>
        private static AirHandlingUnitAirMovement Movement(string name, double lowerLimit, double upperLimit)
        {
            return new AirHandlingUnitAirMovement(
                name,
                null,
                null,
                Humidity(ProfileType.Humidification, lowerLimit),
                Humidity(ProfileType.Dehumidification, upperLimit),
                null);
        }

        private static Profile Humidity(ProfileType profileType, double value)
        {
            return new Profile(string.Format("{0} {1}", profileType.Text(), value), profileType, new double[] { value });
        }

        /// <summary>24 values: <paramref name="value_Morning"/> for hours 0-11, <paramref name="value_Afternoon"/> for 12-23.</summary>
        private static List<double> Hourly(double value_Morning, double value_Afternoon)
        {
            List<double> result = [];
            for (int i = 0; i <= 23; i++)
            {
                result.Add(i < 12 ? value_Morning : value_Afternoon);
            }

            return result;
        }

        /// <summary>
        /// An internal condition naming one humidification and one dehumidification profile, with the
        /// library that resolves them.
        /// </summary>
        private static InternalCondition InternalCondition_Humidity(double lowerLimit, double upperLimit, out ProfileLibrary profileLibrary)
        {
            Profile profile_LowerLimit = Humidity(ProfileType.Humidification, lowerLimit);
            Profile profile_UpperLimit = Humidity(ProfileType.Dehumidification, upperLimit);

            profileLibrary = new ProfileLibrary("Fixture", [profile_LowerLimit, profile_UpperLimit]);

            InternalCondition result = new("Office");
            result.SetValue(InternalConditionParameter.HumidificationProfileName, profile_LowerLimit.Name);
            result.SetValue(InternalConditionParameter.DehumidificationProfileName, profile_UpperLimit.Name);

            return result;
        }

        /// <summary>The records this rule writes, whatever else the log says.</summary>
        internal static List<LogRecord> HumidityRecords(Log log)
        {
            List<LogRecord> result = [];
            foreach (LogRecord logRecord in log ?? new Log())
            {
                if (logRecord?.Text != null && logRecord.Text.Contains("overlapping humid", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(logRecord);
                }
            }

            return result;
        }
    }
}
