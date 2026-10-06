// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>The manufacturer operating-strategy seam: which mode a unit is in, what it then delivers, and at
    /// what airflow - all of it read off data, none of it decided by code.</b>
    /// <para>
    /// This suite pins <see cref="VentilationUnitOperatingStrategy"/> and
    /// <see cref="SupplyTemperatureRule"/>: a vocabulary for the operating strategy a manufacturer states
    /// for representing its own product in a dynamic thermal model. The thresholds, fractions and limits
    /// used below are <b>fixture values</b> on fixture products - nothing here names a real manufacturer,
    /// the same arrangement as <see cref="PartOVentilationUnitTemplateTests"/>, and no shipped catalogue is
    /// read.
    /// </para>
    /// <para>
    /// Two things this suite is careful to prove, because both are ways the work could go quietly wrong:
    /// </para>
    /// <list type="number">
    /// <item><description><b>Exactly one mode, at every boundary.</b> Manufacturers write these conditions
    /// independently, and independently-written conditions leave gaps - a value exactly at a threshold that
    /// two conditions both exclude. The ordered decision has to place every such value in exactly one mode,
    /// and the tests below name each boundary individually rather than sampling around them.</description></item>
    /// <item><description><b>A blend fraction is not a certified efficiency.</b> A modelling rule that says
    /// "four fifths of the way from intake to extract" produces the same arithmetic as an exchanger
    /// effectiveness, and must never be carried as one:
    /// <see cref="VentilationUnitTemplate.HeatRecoveryPerformance"/> stays null when a strategy is
    /// present.</description></item>
    /// </list>
    /// </summary>
    public class PartOVentilationUnitOperatingStrategyTests
    {
        private const string source_Fixture = "Test Fixture, manufacturer modelling guidance, v.1 - not a real product";
        private const double tolerance = 1e-9;

        // =================================================================================================
        // A. The three-state control law, and every boundary of it
        // =================================================================================================

        /// <summary>
        /// The mode at each of the eight stated operating points, with the fixture's 22 &#176;C activation
        /// temperature. These are the cases the strategy exists to get right, listed one by one so a failure
        /// names the case rather than a range.
        /// </summary>
        [Theory]
        [InlineData(10.0, 22.0, VentilationUnitOperatingMode.HeatCoolthRecovery)] //extract at the setpoint: not cooling
        [InlineData(15.0, 22.0, VentilationUnitOperatingMode.SummerBypass)]
        [InlineData(22.0, 22.0, VentilationUnitOperatingMode.HeatCoolthRecovery)] //extract equals intake: not bypass
        [InlineData(15.0, 18.0, VentilationUnitOperatingMode.SummerBypass)] //extract at the bypass minimum: inclusive
        [InlineData(15.0, 17.99, VentilationUnitOperatingMode.HeatCoolthRecovery)] //extract below the bypass minimum
        [InlineData(12.0, 20.0, VentilationUnitOperatingMode.SummerBypass)] //intake at the bypass minimum: inclusive
        [InlineData(11.99, 20.0, VentilationUnitOperatingMode.HeatCoolthRecovery)] //intake below the bypass minimum
        [InlineData(13.0, 20.0, VentilationUnitOperatingMode.SummerBypass)]
        [InlineData(10.0, 22.1, VentilationUnitOperatingMode.Cooling)]
        [InlineData(30.0, 22.1, VentilationUnitOperatingMode.Cooling)] //cooling does not depend on intake
        public void TheControlLaw_SelectsTheStatedMode(double intakeTemperature_C, double extractTemperature_C, VentilationUnitOperatingMode expected)
        {
            Assert.Equal(expected, Strategy().OperatingMode(intakeTemperature_C, extractTemperature_C));
        }

        /// <summary>
        /// Cooling is decided by the extract temperature alone. Over a wide sweep of intake temperatures,
        /// one degree-tenth above the activation temperature is always cooling and the activation
        /// temperature itself never is.
        /// </summary>
        [Fact]
        public void Cooling_DependsOnTheExtractTemperatureAlone()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();

            for (double intakeTemperature_C = -10; intakeTemperature_C <= 40; intakeTemperature_C += 0.5)
            {
                Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingStrategy.OperatingMode(intakeTemperature_C, 22.1));
                Assert.NotEqual(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingStrategy.OperatingMode(intakeTemperature_C, 22.0));
            }
        }

        /// <summary>
        /// Every pair of temperatures selects exactly one mode, and never none. Swept finely enough to hit
        /// every threshold exactly, including the values at which the manufacturer's independently-written
        /// conditions are all false.
        /// </summary>
        [Fact]
        public void TheControlLaw_LeavesNoTemperaturePairWithoutAMode()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();

            for (int intake = -200; intake <= 400; intake++)
            {
                for (int extract = -200; extract <= 400; extract += 1)
                {
                    VentilationUnitOperatingMode ventilationUnitOperatingMode = ventilationUnitOperatingStrategy.OperatingMode(intake / 10.0, extract / 10.0);

                    Assert.NotEqual(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingMode);
                }
            }
        }

        /// <summary>
        /// The bypass requires all three of its conditions. Each is removed in turn, one boundary at a time,
        /// and each removal falls back to recovery rather than to nothing.
        /// </summary>
        [Fact]
        public void TheBypass_RequiresEveryOneOfItsConditions()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();

            Assert.Equal(VentilationUnitOperatingMode.SummerBypass, ventilationUnitOperatingStrategy.OperatingMode(15.0, 21.0));

            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, ventilationUnitOperatingStrategy.OperatingMode(11.99, 21.0)); //intake below the minimum
            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, ventilationUnitOperatingStrategy.OperatingMode(15.0, 15.0)); //extract equal to intake
            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, ventilationUnitOperatingStrategy.OperatingMode(15.0, 14.0)); //extract below intake
            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, ventilationUnitOperatingStrategy.OperatingMode(17.5, 17.99)); //extract below the minimum
        }

        /// <summary>
        /// The activation temperature is data: the same strategy at a second setpoint moves the cooling
        /// boundary with it, and nothing else about the strategy changes.
        /// </summary>
        [Fact]
        public void TheActivationTemperature_IsDataAndMovesTheCoolingBoundary()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy().WithCoolingActivationTemperature(25.0);

            Assert.Null(ventilationUnitOperatingStrategy.Refusal());
            Assert.Equal(25.0, ventilationUnitOperatingStrategy.CoolingActivationTemperature_C);

            Assert.Equal(VentilationUnitOperatingMode.SummerBypass, ventilationUnitOperatingStrategy.OperatingMode(15.0, 22.1));
            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, ventilationUnitOperatingStrategy.OperatingMode(26.0, 25.0));
            Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingStrategy.OperatingMode(10.0, 25.1));

            //The strategy the catalogue stated is untouched by a project that copied it.
            Assert.Equal(22.0, Strategy().CoolingActivationTemperature_C);
        }

        /// <summary>A setpoint outside the range the guidance permits refuses; it is never clamped into it.</summary>
        [Theory]
        [InlineData(21.9)]
        [InlineData(25.1)]
        [InlineData(30.0)]
        public void ASetpointOutsideThePermittedRange_Refuses(double coolingActivationTemperature_C)
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy().WithCoolingActivationTemperature(coolingActivationTemperature_C);

            Assert.NotNull(ventilationUnitOperatingStrategy.Refusal());
            Assert.Contains("cooling activation temperature", ventilationUnitOperatingStrategy.Refusal());
            Assert.Equal(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingStrategy.OperatingMode(15.0, 21.0));
        }

        /// <summary>A temperature that is not a number selects no mode - it does not fall through to the default one.</summary>
        [Fact]
        public void ATemperatureThatIsNotANumber_SelectsNoMode()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();

            Assert.Equal(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingStrategy.OperatingMode(double.NaN, 21.0));
            Assert.Equal(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingStrategy.OperatingMode(15.0, double.NaN));
            Assert.Equal(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingStrategy.OperatingMode(double.PositiveInfinity, 21.0));
        }

        // =================================================================================================
        // B. What each mode delivers - the package supply temperature
        // =================================================================================================

        /// <summary>Bypassing delivers intake air, at every intake temperature.</summary>
        [Theory]
        [InlineData(13.0, 20.0)]
        [InlineData(15.0, 21.0)]
        [InlineData(19.0, 21.5)]
        public void Bypassing_DeliversIntakeAir(double intakeTemperature_C, double extractTemperature_C)
        {
            double supplyTemperature_C = Strategy().SupplyTemperature(intakeTemperature_C, extractTemperature_C, 30.0, Table(), out VentilationUnitOperatingMode ventilationUnitOperatingMode, out double operatingAirFlowRate_Lps);

            Assert.Equal(VentilationUnitOperatingMode.SummerBypass, ventilationUnitOperatingMode);
            Assert.Equal(intakeTemperature_C, supplyTemperature_C, tolerance);
            Assert.Equal(30.0, operatingAirFlowRate_Lps, tolerance);
        }

        /// <summary>Recovering delivers the stated blend of extract and intake air.</summary>
        [Theory]
        [InlineData(0.0, 20.0, 16.0)]
        [InlineData(10.0, 20.0, 18.0)]
        [InlineData(22.0, 22.0, 22.0)]
        [InlineData(25.0, 15.0, 17.0)] //coolth recovery: intake above extract
        public void Recovering_DeliversTheStatedBlend(double intakeTemperature_C, double extractTemperature_C, double expected_C)
        {
            double supplyTemperature_C = Strategy().SupplyTemperature(intakeTemperature_C, extractTemperature_C, 30.0, Table(), out VentilationUnitOperatingMode ventilationUnitOperatingMode, out _);

            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, ventilationUnitOperatingMode);
            Assert.Equal(expected_C, supplyTemperature_C, tolerance);
            Assert.Equal((0.8 * extractTemperature_C) + (0.2 * intakeTemperature_C), supplyTemperature_C, tolerance);
        }

        /// <summary>
        /// Cooling delivers what the published table states at this hour's intake temperature, extract
        /// temperature and <b>elevated</b> airflow - not at the design airflow.
        /// </summary>
        [Fact]
        public void Cooling_DeliversThePublishedTableAtTheElevatedAirflow()
        {
            double supplyTemperature_C = Strategy().SupplyTemperature(30.0, 24.0, 30.0, Table(), out VentilationUnitOperatingMode ventilationUnitOperatingMode, out double operatingAirFlowRate_Lps);

            Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingMode);
            Assert.Equal(80.0, operatingAirFlowRate_Lps, tolerance);

            //The fixture table states 18.0 degC at 30 degC intake, 24 degC extract, 80 l/s.
            Assert.Equal(18.0, supplyTemperature_C, tolerance);
        }

        /// <summary>
        /// A stated minimum supply temperature is a floor on what the rule computes. The published cell it
        /// floors is left exactly as published - the table still answers its own number.
        /// </summary>
        [Fact]
        public void AStatedMinimumSupplyTemperature_FloorsTheAnswerAndNotTheTable()
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = Table();

            //The fixture table states 14.0 degC at 25 degC intake, 23 degC extract, 80 l/s - below the floor.
            Assert.Equal(14.0, ventilationUnitPerformanceTable.Value(VentilationUnitPerformanceOutput.Name_SupplyAirTemperature, [25.0, 23.0, 80.0], PerformanceDomainPolicy.ClampToDomain), tolerance);

            double supplyTemperature_C = Strategy().SupplyTemperature(25.0, 23.0, 30.0, ventilationUnitPerformanceTable, out VentilationUnitOperatingMode ventilationUnitOperatingMode, out _);

            Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingMode);
            Assert.Equal(16.0, supplyTemperature_C, tolerance);

            //Without the floor, the same rule answers the published figure.
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule = SupplyTemperatureRule.PerformanceTable();

            Assert.Equal(14.0, ventilationUnitOperatingStrategy.SupplyTemperature(25.0, 23.0, 30.0, ventilationUnitPerformanceTable, out _, out _), tolerance);
        }

        /// <summary>
        /// A cooling table published over summer design conditions is asked about hours outside them, so the
        /// rule holds at the published edges rather than extending the data - and a rule that says it should
        /// refuse instead answers nothing.
        /// </summary>
        [Fact]
        public void OutsideThePublishedGrid_TheRuleHoldsAtItsEdgesOrRefuses_AsItStates()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();

            //15 degC intake is far below the table's lowest published intake temperature of 25 degC.
            double supplyTemperature_C = ventilationUnitOperatingStrategy.SupplyTemperature(15.0, 24.0, 30.0, Table(), out VentilationUnitOperatingMode ventilationUnitOperatingMode, out _);

            Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingMode);
            Assert.Equal(ventilationUnitOperatingStrategy.SupplyTemperature(25.0, 24.0, 30.0, Table(), out _, out _), supplyTemperature_C, tolerance);

            ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule = SupplyTemperatureRule.PerformanceTable(16.0, PerformanceDomainPolicy.Refuse);

            Assert.True(double.IsNaN(ventilationUnitOperatingStrategy.SupplyTemperature(15.0, 24.0, 30.0, Table(), out _, out _)));
        }

        /// <summary>A cooling rule with no table to read answers nothing; it does not fall back to a blend.</summary>
        [Fact]
        public void ACoolingRuleWithNoTable_AnswersNothing()
        {
            Assert.True(double.IsNaN(Strategy().SupplyTemperature(30.0, 24.0, 30.0, null, out VentilationUnitOperatingMode ventilationUnitOperatingMode, out _)));
            Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingMode);
        }

        // =================================================================================================
        // C. Airflow: background is the design rate, cooling is the elevated rate, and neither becomes the other
        // =================================================================================================

        /// <summary>
        /// The background modes move what the dwelling was designed to move - whatever that is - and cooling
        /// moves the stated elevated rate, which does not vary with the design rate.
        /// </summary>
        [Theory]
        [InlineData(19.0)]
        [InlineData(30.0)]
        [InlineData(47.5)]
        public void BackgroundModes_MoveTheDesignRate_AndCoolingMovesTheElevatedRate(double designAirFlowRate_Lps)
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();

            Assert.Equal(designAirFlowRate_Lps, ventilationUnitOperatingStrategy.OperatingAirFlowRate_Lps(VentilationUnitOperatingMode.HeatCoolthRecovery, designAirFlowRate_Lps), tolerance);
            Assert.Equal(designAirFlowRate_Lps, ventilationUnitOperatingStrategy.OperatingAirFlowRate_Lps(VentilationUnitOperatingMode.SummerBypass, designAirFlowRate_Lps), tolerance);
            Assert.Equal(80.0, ventilationUnitOperatingStrategy.OperatingAirFlowRate_Lps(VentilationUnitOperatingMode.Cooling, designAirFlowRate_Lps), tolerance);

            //The elevated rate is an operating airflow. It is not written anywhere near the design one.
            Assert.Equal(80.0, ventilationUnitOperatingStrategy.ElevatedAirFlow_Lps, tolerance);
        }

        /// <summary>A design airflow that is not a usable number is a refusal, not a zero.</summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(0.0)]
        [InlineData(-5.0)]
        public void ABackgroundModeWithNoUsableDesignRate_AnswersNothing(double designAirFlowRate_Lps)
        {
            Assert.True(double.IsNaN(Strategy().OperatingAirFlowRate_Lps(VentilationUnitOperatingMode.HeatCoolthRecovery, designAirFlowRate_Lps)));
        }

        /// <summary>
        /// A catalogue states a manufacturer's thresholds, rules and range; it does not state how far up
        /// that range one dwelling goes. A strategy without a resolved elevated airflow is therefore a
        /// complete record of the guidance - and is not yet something a unit can be operated on.
        /// </summary>
        [Fact]
        public void AStrategyWithNoResolvedElevatedAirflow_IsACompleteRecordAndNotAnOperatingStrategy()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.ElevatedAirFlow_Lps = double.NaN;

            Assert.False(ventilationUnitOperatingStrategy.IsResolved);
            Assert.Null(ventilationUnitOperatingStrategy.TemplateRefusal());

            Assert.NotNull(ventilationUnitOperatingStrategy.Refusal());
            Assert.Contains("elevated cooling airflow", ventilationUnitOperatingStrategy.Refusal());

            //Nothing operates until it is resolved - no mode, no supply temperature, no airflow.
            Assert.Equal(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingStrategy.OperatingMode(15.0, 21.0));
            Assert.True(double.IsNaN(ventilationUnitOperatingStrategy.OperatingAirFlowRate_Lps(VentilationUnitOperatingMode.HeatCoolthRecovery, 30.0)));

            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy_Resolved = ventilationUnitOperatingStrategy.WithElevatedAirFlow(75.0);

            Assert.True(ventilationUnitOperatingStrategy_Resolved.IsResolved);
            Assert.Null(ventilationUnitOperatingStrategy_Resolved.Refusal());
            Assert.Equal(75.0, ventilationUnitOperatingStrategy_Resolved.OperatingAirFlowRate_Lps(VentilationUnitOperatingMode.Cooling, 30.0), tolerance);

            //And the record it was resolved from is untouched.
            Assert.False(ventilationUnitOperatingStrategy.IsResolved);
        }

        /// <summary>
        /// A stated elevated airflow outside the manufacturer's range refuses the record itself, so a
        /// catalogue cannot state an impossible one and have it found only at operating time.
        /// </summary>
        [Fact]
        public void AnElevatedAirflowOutsideTheStatedRange_RefusesTheRecordItself()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy().WithElevatedAirFlow(95.0);

            Assert.NotNull(ventilationUnitOperatingStrategy.TemplateRefusal());
            Assert.Contains("elevated cooling airflow", ventilationUnitOperatingStrategy.TemplateRefusal());
        }

        /// <summary>An elevated airflow outside the range the guidance states refuses.</summary>
        [Theory]
        [InlineData(65.0)]
        [InlineData(95.0)]
        public void AnElevatedAirflowOutsideTheStatedRange_Refuses(double elevatedAirFlow_Lps)
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.ElevatedAirFlow_Lps = elevatedAirFlow_Lps;

            Assert.NotNull(ventilationUnitOperatingStrategy.Refusal());
            Assert.Contains("elevated cooling airflow", ventilationUnitOperatingStrategy.Refusal());
        }

        // =================================================================================================
        // D. Refusals: every one of them a refusal, none of them a repair
        // =================================================================================================

        /// <summary>A strategy nobody can trace to a manufacturer's own guidance is not manufacturer guidance.</summary>
        [Fact]
        public void AStrategyWithNoSource_Refuses()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.Source = null;

            Assert.NotNull(ventilationUnitOperatingStrategy.Refusal());
            Assert.Contains("source", ventilationUnitOperatingStrategy.Refusal());
        }

        /// <summary>A mode with no rule refuses the strategy; it does not borrow another mode's rule.</summary>
        [Fact]
        public void AModeWithNoRule_Refuses()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.SummerBypassSupplyTemperatureRule = null;

            Assert.NotNull(ventilationUnitOperatingStrategy.Refusal());
            Assert.Contains("Summer bypass", ventilationUnitOperatingStrategy.Refusal());
            Assert.True(double.IsNaN(ventilationUnitOperatingStrategy.SupplyTemperature(15.0, 21.0, 30.0, Table(), out _, out _)));
        }

        /// <summary>A blend fraction outside the unit interval is a transcription mistake, and refuses.</summary>
        [Theory]
        [InlineData(-0.1)]
        [InlineData(1.1)]
        [InlineData(double.NaN)]
        public void ABlendFractionOutsideZeroToOne_Refuses(double extractFraction)
        {
            SupplyTemperatureRule supplyTemperatureRule = SupplyTemperatureRule.LinearBlend(extractFraction);

            Assert.NotNull(supplyTemperatureRule.Refusal());
            Assert.True(double.IsNaN(supplyTemperatureRule.SupplyTemperature(10.0, 20.0, 30.0)));
        }

        /// <summary>A half-stated elevated airflow range refuses rather than being read as no range at all.</summary>
        [Fact]
        public void AHalfStatedElevatedAirflowRange_Refuses()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.MaximumElevatedAirFlow_Lps = double.NaN;

            Assert.NotNull(ventilationUnitOperatingStrategy.Refusal());
            Assert.Contains("one end", ventilationUnitOperatingStrategy.Refusal());
        }

        /// <summary>A missing threshold refuses; nothing is filled in.</summary>
        [Fact]
        public void AMissingThreshold_Refuses()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.BypassMinimumIntakeTemperature_C = double.NaN;

            Assert.NotNull(ventilationUnitOperatingStrategy.Refusal());
            Assert.Contains("bypass minimum intake temperature", ventilationUnitOperatingStrategy.Refusal());
        }

        // =================================================================================================
        // E. Round trip, and the line between guidance and certified data
        // =================================================================================================

        /// <summary>A strategy survives a JSON round trip on a template, value for value.</summary>
        [Fact]
        public void AStrategy_SurvivesARoundTripOnItsTemplate()
        {
            VentilationUnitTemplate ventilationUnitTemplate = new(new VentilationUnitReference("Fixture Manufacturer", "UNIT-A", "COOL-A"), source_Fixture)
            {
                MaximumSupplyFlowRate_Lps = 150,
                MaximumExtractFlowRate_Lps = 150,
                PerformanceTable = Table(),
                OperatingStrategy = Strategy(),
            };

            JsonObject jsonObject = ventilationUnitTemplate.ToJsonObject();

            VentilationUnitTemplate ventilationUnitTemplate_RoundTrip = new(jsonObject);
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = ventilationUnitTemplate_RoundTrip.OperatingStrategy;

            Assert.NotNull(ventilationUnitOperatingStrategy);
            Assert.Null(ventilationUnitOperatingStrategy.Refusal());

            Assert.Equal(source_Fixture, ventilationUnitOperatingStrategy.Source);
            Assert.Equal(22.0, ventilationUnitOperatingStrategy.CoolingActivationTemperature_C, tolerance);
            Assert.Equal(22.0, ventilationUnitOperatingStrategy.MinimumCoolingActivationTemperature_C, tolerance);
            Assert.Equal(25.0, ventilationUnitOperatingStrategy.MaximumCoolingActivationTemperature_C, tolerance);
            Assert.Equal(12.0, ventilationUnitOperatingStrategy.BypassMinimumIntakeTemperature_C, tolerance);
            Assert.Equal(18.0, ventilationUnitOperatingStrategy.BypassMinimumExtractTemperature_C, tolerance);
            Assert.Equal(80.0, ventilationUnitOperatingStrategy.ElevatedAirFlow_Lps, tolerance);
            Assert.Equal(70.0, ventilationUnitOperatingStrategy.MinimumElevatedAirFlow_Lps, tolerance);
            Assert.Equal(90.0, ventilationUnitOperatingStrategy.MaximumElevatedAirFlow_Lps, tolerance);

            Assert.Equal(SupplyTemperatureRuleType.OutdoorAir, ventilationUnitOperatingStrategy.SummerBypassSupplyTemperatureRule.SupplyTemperatureRuleType);
            Assert.Equal(SupplyTemperatureRuleType.LinearBlend, ventilationUnitOperatingStrategy.HeatCoolthRecoverySupplyTemperatureRule.SupplyTemperatureRuleType);
            Assert.Equal(0.8, ventilationUnitOperatingStrategy.HeatCoolthRecoverySupplyTemperatureRule.ExtractFraction, tolerance);
            Assert.Equal(SupplyTemperatureRuleType.PerformanceTable, ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule.SupplyTemperatureRuleType);
            Assert.Equal(16.0, ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule.MinimumSupplyTemperature_C, tolerance);
            Assert.Equal(PerformanceDomainPolicy.ClampToDomain, ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule.PerformanceDomainPolicy);

            //Every mode still selects as it did before the round trip.
            Assert.Equal(VentilationUnitOperatingMode.SummerBypass, ventilationUnitOperatingStrategy.OperatingMode(15.0, 22.0));
            Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingStrategy.OperatingMode(15.0, 22.1));
        }

        /// <summary>
        /// A domain policy that is present but is not one of the stated names refuses the rule, rather than
        /// quietly becoming the permissive one - the direction a typo must never take.
        /// </summary>
        [Fact]
        public void AnUnrecognisedDomainPolicy_RefusesTheRule()
        {
            JsonObject jsonObject = SupplyTemperatureRule.PerformanceTable(16.0).ToJsonObject();
            jsonObject["PerformanceDomainPolicy"] = "Clamp";

            SupplyTemperatureRule supplyTemperatureRule = new(jsonObject);

            Assert.NotNull(supplyTemperatureRule.Refusal());
            Assert.True(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 80.0, Table())));
        }

        /// <summary>
        /// A template carrying a manufacturer's modelling strategy carries <b>no</b> certified heat-recovery
        /// or fan performance because of it. The blend fraction a strategy states is manufacturer guidance,
        /// and the certified fields stay exactly as empty as they were.
        /// </summary>
        [Fact]
        public void AStrategy_IsNotCertifiedPerformance()
        {
            VentilationUnitTemplate ventilationUnitTemplate = new(new VentilationUnitReference("Fixture Manufacturer", "UNIT-A", "COOL-A"), source_Fixture)
            {
                OperatingStrategy = Strategy(),
            };

            Assert.NotNull(ventilationUnitTemplate.OperatingStrategy);
            Assert.Null(ventilationUnitTemplate.HeatRecoveryPerformance);
            Assert.Null(ventilationUnitTemplate.FanPerformance);

            VentilationUnitTemplate ventilationUnitTemplate_RoundTrip = new(ventilationUnitTemplate.ToJsonObject());

            Assert.NotNull(ventilationUnitTemplate_RoundTrip.OperatingStrategy);
            Assert.Null(ventilationUnitTemplate_RoundTrip.HeatRecoveryPerformance);
            Assert.Null(ventilationUnitTemplate_RoundTrip.FanPerformance);
        }

        /// <summary>A template written before strategies existed reads exactly as it always did.</summary>
        [Fact]
        public void ATemplateWithoutAStrategy_ReadsAsItAlwaysDid()
        {
            VentilationUnitTemplate ventilationUnitTemplate = new(new VentilationUnitReference("Fixture Manufacturer", "UNIT-B", "UNIT-B"), source_Fixture)
            {
                MaximumSupplyFlowRate_Lps = 100,
                MaximumExtractFlowRate_Lps = 100,
            };

            JsonObject jsonObject = ventilationUnitTemplate.ToJsonObject();

            Assert.False(jsonObject.ContainsKey("OperatingStrategy"));
            Assert.Null(new VentilationUnitTemplate(jsonObject).OperatingStrategy);
        }

        /// <summary>A copy of a strategy is a copy: changing it never reaches the original.</summary>
        [Fact]
        public void ACopiedStrategy_IsIndependentOfItsOriginal()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy_Copy = new(ventilationUnitOperatingStrategy);

            ventilationUnitOperatingStrategy_Copy.ElevatedAirFlow_Lps = 90.0;
            ventilationUnitOperatingStrategy_Copy.CoolingSupplyTemperatureRule = SupplyTemperatureRule.OutdoorAir();

            Assert.Equal(80.0, ventilationUnitOperatingStrategy.ElevatedAirFlow_Lps, tolerance);
            Assert.Equal(SupplyTemperatureRuleType.PerformanceTable, ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule.SupplyTemperatureRuleType);
        }

        // =================================================================================================
        // F. Intake-offset cooling rule, and a cooling-stat separate from the extract
        // =================================================================================================

        /// <summary>
        /// <c>intake - X(airflow)</c> at the stated airflows, linear between them, and applied to the intake
        /// whatever the extract is - the package supply of a guidance rule that collapses two cooling stages
        /// into one offset.
        /// </summary>
        [Theory]
        [InlineData(30.0, 70.0, 15.0)]
        [InlineData(30.0, 80.0, 16.0)]
        [InlineData(30.0, 75.0, 15.5)]
        [InlineData(13.0, 90.0, 0.0)]
        public void AnIntakeOffset_DeliversIntakeLessTheOffsetAtTheAirflow(double intakeTemperature_C, double airFlowRate_Lps, double expected_C)
        {
            SupplyTemperatureRule supplyTemperatureRule = IntakeOffsetRule();

            Assert.Null(supplyTemperatureRule.Refusal());
            Assert.Equal(expected_C, supplyTemperatureRule.SupplyTemperature(intakeTemperature_C, 24.0, airFlowRate_Lps), tolerance);
            Assert.Equal(expected_C, supplyTemperatureRule.SupplyTemperature(intakeTemperature_C, 35.0, airFlowRate_Lps), tolerance);
            Assert.Null(supplyTemperatureRule.AirFlowDomainCondition(airFlowRate_Lps));
        }

        /// <summary>
        /// Outside the stated airflows the default answers nothing; a rule that states clamping holds the
        /// nearest offset - and in both cases the use is reported, because clamping says what number, not
        /// that the number was stated.
        /// </summary>
        [Fact]
        public void OutsideTheStatedAirflows_TheOffsetRefusesOrHolds_AndIsAlwaysReported()
        {
            SupplyTemperatureRule supplyTemperatureRule_Refuse = IntakeOffsetRule();
            SupplyTemperatureRule supplyTemperatureRule_Clamp = SupplyTemperatureRule.IntakeOffset([70.0, 80.0, 90.0], [15.0, 14.0, 13.0], double.NaN, PerformanceDomainPolicy.ClampToDomain);

            Assert.Equal(PerformanceDomainPolicy.Refuse, supplyTemperatureRule_Refuse.PerformanceDomainPolicy);
            Assert.True(double.IsNaN(supplyTemperatureRule_Refuse.SupplyTemperature(30.0, 24.0, 30.0)));
            Assert.Equal(15.0, supplyTemperatureRule_Clamp.SupplyTemperature(30.0, 24.0, 30.0), tolerance);
            Assert.Equal(17.0, supplyTemperatureRule_Clamp.SupplyTemperature(30.0, 24.0, 120.0), tolerance);

            Assert.NotNull(supplyTemperatureRule_Refuse.AirFlowDomainCondition(30.0));
            Assert.NotNull(supplyTemperatureRule_Clamp.AirFlowDomainCondition(30.0));
            Assert.NotNull(supplyTemperatureRule_Clamp.AirFlowDomainCondition(120.0));
        }

        /// <summary>No lower limit is applied unless one is stated; a stated one floors the answer.</summary>
        [Fact]
        public void AnIntakeOffset_HasNoFloorUnlessOneIsStated()
        {
            Assert.Equal(-3.0, IntakeOffsetRule().SupplyTemperature(12.0, 24.0, 70.0), tolerance);
            Assert.Equal(10.0, SupplyTemperatureRule.IntakeOffset([70.0], [14.0], 10.0).SupplyTemperature(12.0, 24.0, 70.0), tolerance);
        }

        /// <summary>A mistranscribed offset table is refused, never repaired.</summary>
        [Theory]
        [InlineData(new double[] { 70.0, 80.0 }, new double[] { 15.0 })]
        [InlineData(new double[] { 80.0, 70.0 }, new double[] { 15.0, 14.0 })]
        [InlineData(new double[] { 0.0, 70.0 }, new double[] { 15.0, 14.0 })]
        [InlineData(new double[] { }, new double[] { })]
        public void AMistranscribedOffsetTable_Refuses(double[] airFlowRates_Lps, double[] intakeOffsets_K)
        {
            SupplyTemperatureRule supplyTemperatureRule = SupplyTemperatureRule.IntakeOffset(airFlowRates_Lps, intakeOffsets_K);

            Assert.NotNull(supplyTemperatureRule.Refusal());
            Assert.True(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 70.0)));
        }

        /// <summary>The offset table survives a JSON round trip, cell for cell, with its policy.</summary>
        [Fact]
        public void AnIntakeOffset_SurvivesARoundTrip()
        {
            SupplyTemperatureRule supplyTemperatureRule = new(IntakeOffsetRule().ToJsonObject());

            Assert.Equal(SupplyTemperatureRuleType.IntakeOffset, supplyTemperatureRule.SupplyTemperatureRuleType);
            Assert.Equal(new[] { 70.0, 80.0, 90.0 }, supplyTemperatureRule.AirFlowRates_Lps);
            Assert.Equal(new[] { 15.0, 14.0, 13.0 }, supplyTemperatureRule.IntakeOffsets_K);
            Assert.Equal(PerformanceDomainPolicy.Refuse, supplyTemperatureRule.PerformanceDomainPolicy);
            Assert.True(double.IsNaN(supplyTemperatureRule.MinimumSupplyTemperature_C));
        }

        /// <summary>
        /// A room-stat strategy cools on the room and nothing else: a warm extract with a cool room does not
        /// cool, a warm room with a cool extract does, and bypass/recovery still read the extract.
        /// </summary>
        [Fact]
        public void ARoomStatStrategy_CoolsOnTheRoom_AndBypassesOnTheExtract()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.CoolingActivationSignal = CoolingActivationSignal.RoomTemperature;

            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, ventilationUnitOperatingStrategy.OperatingMode(10.0, 25.0, 20.0));
            Assert.Equal(VentilationUnitOperatingMode.Cooling, ventilationUnitOperatingStrategy.OperatingMode(10.0, 19.0, 22.5));
            Assert.Equal(VentilationUnitOperatingMode.SummerBypass, ventilationUnitOperatingStrategy.OperatingMode(15.0, 20.0, 21.0));
            Assert.Equal(VentilationUnitOperatingMode.SummerBypass, ventilationUnitOperatingStrategy.OperatingMode(15.0, 20.0, 22.0));

            //The bypass is the unit's own decision, independent of the cooling-stat (Nuaire, 24 Sep 2026): a warm
            //extract with a cool room bypasses where the bypass conditions hold.
            Assert.Equal(VentilationUnitOperatingMode.SummerBypass, ventilationUnitOperatingStrategy.OperatingMode(15.0, 25.0, 20.0));
        }

        /// <summary>
        /// A room-stat strategy is never quietly evaluated on the extract: without a room temperature it
        /// selects no mode.
        /// </summary>
        [Fact]
        public void ARoomStatStrategy_WithoutARoomTemperature_SelectsNoMode()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.CoolingActivationSignal = CoolingActivationSignal.RoomTemperature;

            Assert.Equal(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingStrategy.OperatingMode(10.0, 25.0));
            Assert.Equal(VentilationUnitOperatingMode.Undefined, ventilationUnitOperatingStrategy.OperatingMode(10.0, 25.0, double.NaN));
        }

        /// <summary>
        /// The default is the extract, as before; the signal round-trips; an absent signal reads as the
        /// extract and an unrecognised one refuses.
        /// </summary>
        [Fact]
        public void TheCoolingActivationSignal_DefaultsToTheExtract_RoundTrips_AndRefusesAnUnknownName()
        {
            Assert.Equal(CoolingActivationSignal.ExtractTemperature, Strategy().CoolingActivationSignal);

            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.CoolingActivationSignal = CoolingActivationSignal.RoomTemperature;
            ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule = IntakeOffsetRule();

            JsonObject jsonObject = ventilationUnitOperatingStrategy.ToJsonObject();
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy_RoundTrip = new(jsonObject);

            Assert.Equal(CoolingActivationSignal.RoomTemperature, ventilationUnitOperatingStrategy_RoundTrip.CoolingActivationSignal);
            Assert.Equal(SupplyTemperatureRuleType.IntakeOffset, ventilationUnitOperatingStrategy_RoundTrip.CoolingSupplyTemperatureRule.SupplyTemperatureRuleType);
            Assert.Null(ventilationUnitOperatingStrategy_RoundTrip.Refusal());

            jsonObject.Remove("CoolingActivationSignal");
            Assert.Equal(CoolingActivationSignal.ExtractTemperature, new VentilationUnitOperatingStrategy(jsonObject).CoolingActivationSignal);

            jsonObject["CoolingActivationSignal"] = "Wall";
            Assert.NotNull(new VentilationUnitOperatingStrategy(jsonObject).Refusal());
        }

        /// <summary>
        /// A room-stat, intake-offset strategy states the supply at the elevated airflow while cooling and
        /// the background rules otherwise.
        /// </summary>
        [Fact]
        public void ARoomStatIntakeOffsetStrategy_DeliversIntakeLessXAtTheElevatedAirflow()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.CoolingActivationSignal = CoolingActivationSignal.RoomTemperature;
            ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule = IntakeOffsetRule();

            double supply_Cooling = ventilationUnitOperatingStrategy.SupplyTemperature(28.0, 24.0, 23.0, 30.0, null, out VentilationUnitOperatingMode mode_Cooling, out double flow_Cooling);
            Assert.Equal(VentilationUnitOperatingMode.Cooling, mode_Cooling);
            Assert.Equal(80.0, flow_Cooling, tolerance);
            Assert.Equal(14.0, supply_Cooling, tolerance);

            double supply_Recovery = ventilationUnitOperatingStrategy.SupplyTemperature(5.0, 24.0, 21.0, 30.0, null, out VentilationUnitOperatingMode mode_Recovery, out double flow_Recovery);
            Assert.Equal(VentilationUnitOperatingMode.HeatCoolthRecovery, mode_Recovery);
            Assert.Equal(30.0, flow_Recovery, tolerance);
            Assert.Equal((0.8 * 24.0) + (0.2 * 5.0), supply_Recovery, tolerance);
        }

        // =================================================================================================
        // G. Exchanger, then a coil drop, with a lower limit (Nuaire, 24 Sep 2026)
        // =================================================================================================

        /// <summary>
        /// At 90 l/s the rule is the manufacturer's switching formula: coolth recovery at the stated fraction
        /// then the net coil drop, or bypass then the net coil drop. The interpolated figures are 0.8466 and
        /// 9.265..7.705 K less 0.3..1.1 K = 7.835 K, which the formula rounds to 7.84 K.
        /// </summary>
        [Theory]
        [InlineData(32.0, 24.5, false)]
        [InlineData(28.0, 24.0, false)]
        [InlineData(25.0, 26.0, true)]
        [InlineData(22.0, 23.0, true)]
        public void AnExchangerThenCoil_At90Lps_IsTheManufacturerFormula(double intake_C, double extract_C, bool bypassed)
        {
            SupplyTemperatureRule supplyTemperatureRule = ExchangerThenCoilRule();

            Assert.Null(supplyTemperatureRule.Refusal());
            Assert.Equal(0.8466, supplyTemperatureRule.ExchangerExtractFraction(90.0), 1e-9);
            Assert.Equal(7.835, supplyTemperatureRule.CoilNetTemperatureDrop_K(90.0), 1e-9);

            double expected_C = (bypassed ? intake_C : (0.8466 * extract_C) + (0.1534 * intake_C)) - 7.835;
            Assert.Equal(expected_C, supplyTemperatureRule.SupplyTemperature(intake_C, extract_C, 90.0, bypassed), tolerance);

            double formula_C = bypassed ? intake_C - 7.84 : (0.8466 * extract_C) + (0.1534 * intake_C) - 7.84;
            Assert.Equal(formula_C, supplyTemperatureRule.SupplyTemperature(intake_C, extract_C, 90.0, bypassed), 0.006);
        }

        /// <summary>The stated figures at the stated airflows, and linear between them.</summary>
        [Theory]
        [InlineData(60.0, 0.8796, 8.965)]
        [InlineData(80.0, 0.8576, 8.245)]
        [InlineData(100.0, 0.8356, 7.425)]
        [InlineData(120.0, 0.8136, 6.605)]
        [InlineData(70.0, 0.8686, 8.605)]
        public void AnExchangerThenCoil_InterpolatesItsFiguresOverAirflow(double airFlowRate_Lps, double extractFraction, double netDrop_K)
        {
            SupplyTemperatureRule supplyTemperatureRule = ExchangerThenCoilRule();

            Assert.Equal(extractFraction, supplyTemperatureRule.ExchangerExtractFraction(airFlowRate_Lps), 1e-9);
            Assert.Equal(netDrop_K, supplyTemperatureRule.CoilNetTemperatureDrop_K(airFlowRate_Lps), 1e-9);
            Assert.Null(supplyTemperatureRule.AirFlowDomainCondition(airFlowRate_Lps));
        }

        /// <summary>
        /// The limit bounds what the coil produces, at the boundary exactly: just above the kink the full drop
        /// applies, at and below it the coil delivers the limit, and air that already leaves the exchanger at or
        /// below the limit passes through - a coil does not heat.
        /// </summary>
        [Theory]
        [InlineData(21.345, 13.1)]
        [InlineData(21.245, 13.0)]
        [InlineData(20.0, 13.0)]
        [InlineData(13.0, 13.0)]
        [InlineData(12.5, 12.5)]
        public void AnExchangerThenCoil_NeverCoolsBelowTheLimit_AndNeverHeats(double exchangerLeaving_C, double expected_C)
        {
            //Bypassed, so the exchanger leaving temperature is the intake temperature.
            Assert.Equal(expected_C, ExchangerThenCoilRule().SupplyTemperature(exchangerLeaving_C, 30.0, 80.0, true), tolerance);
        }

        /// <summary>
        /// Outside the stated 60-120 l/s nothing is invented: the default refuses, and the use is reported. The
        /// table-free call answers nothing, because the answer depends on the exchanger state.
        /// </summary>
        [Fact]
        public void AnExchangerThenCoil_RefusesOutsideItsAirflows_AndNeedsTheExchangerState()
        {
            SupplyTemperatureRule supplyTemperatureRule = ExchangerThenCoilRule();

            Assert.Equal(PerformanceDomainPolicy.Refuse, supplyTemperatureRule.PerformanceDomainPolicy);
            Assert.True(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 59.9, false)));
            Assert.True(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 120.1, false)));
            Assert.NotNull(supplyTemperatureRule.AirFlowDomainCondition(59.9));
            Assert.NotNull(supplyTemperatureRule.AirFlowDomainCondition(120.1));
            Assert.False(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 60.0, false)));
            Assert.False(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 120.0, false)));

            Assert.True(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 80.0)));
        }

        /// <summary>A mistranscribed exchanger and coil table is refused, never repaired.</summary>
        [Theory]
        [InlineData(new double[] { 60.0, 80.0 }, new double[] { 0.88 }, new double[] { 9.0, 8.7 }, new double[] { 0.3, 0.5 })]
        [InlineData(new double[] { 80.0, 60.0 }, new double[] { 0.88, 0.86 }, new double[] { 9.0, 8.7 }, new double[] { 0.3, 0.5 })]
        [InlineData(new double[] { 60.0, 80.0 }, new double[] { 1.2, 0.86 }, new double[] { 9.0, 8.7 }, new double[] { 0.3, 0.5 })]
        [InlineData(new double[] { 60.0, 80.0 }, new double[] { 0.88, 0.86 }, new double[] { 0.2, 8.7 }, new double[] { 0.3, 0.5 })]
        [InlineData(new double[] { 60.0, 80.0 }, new double[] { 0.88, 0.86 }, new double[] { 9.0, 8.7 }, new double[] { -0.1, 0.5 })]
        public void AMistranscribedExchangerThenCoil_Refuses(double[] airFlowRates_Lps, double[] extractFractions, double[] coilDrops_K, double[] fanRises_K)
        {
            SupplyTemperatureRule supplyTemperatureRule = SupplyTemperatureRule.ExchangerThenCoil(airFlowRates_Lps, extractFractions, coilDrops_K, fanRises_K, 13.0);

            Assert.NotNull(supplyTemperatureRule.Refusal());
            Assert.True(double.IsNaN(supplyTemperatureRule.SupplyTemperature(30.0, 24.0, 70.0, false)));
        }

        /// <summary>The exchanger and coil table survives a JSON round trip, with its limit and policy.</summary>
        [Fact]
        public void AnExchangerThenCoil_SurvivesARoundTrip()
        {
            SupplyTemperatureRule supplyTemperatureRule = new(ExchangerThenCoilRule().ToJsonObject());

            Assert.Equal(SupplyTemperatureRuleType.ExchangerThenCoil, supplyTemperatureRule.SupplyTemperatureRuleType);
            Assert.Equal(new[] { 60.0, 80.0, 100.0, 120.0 }, supplyTemperatureRule.AirFlowRates_Lps);
            Assert.Equal(new[] { 0.8796, 0.8576, 0.8356, 0.8136 }, supplyTemperatureRule.ExtractFractions);
            Assert.Equal(new[] { 9.265, 8.745, 8.225, 7.705 }, supplyTemperatureRule.CoilTemperatureDrops_K);
            Assert.Equal(new[] { 0.3, 0.5, 0.8, 1.1 }, supplyTemperatureRule.FanTemperatureRises_K);
            Assert.Equal(13.0, supplyTemperatureRule.MinimumSupplyTemperature_C);
            Assert.Equal(PerformanceDomainPolicy.Refuse, supplyTemperatureRule.PerformanceDomainPolicy);
            Assert.Null(supplyTemperatureRule.Refusal());
        }

        /// <summary>The bypass decision alone: inclusive at both minimums, strict on extract above intake.</summary>
        [Theory]
        [InlineData(11.99, 20.0, false)]
        [InlineData(12.0, 20.0, true)]
        [InlineData(15.0, 17.99, false)]
        [InlineData(15.0, 18.0, true)]
        [InlineData(20.0, 20.0, false)]
        [InlineData(26.0, 24.0, false)]
        public void TheExchangerBypass_IsItsStatedConditionsAlone(double intake_C, double extract_C, bool expected)
        {
            Assert.Equal(expected, Strategy().ExchangerBypassed(intake_C, extract_C));
        }

        /// <summary>
        /// A room-stat strategy with an exchanger-then-coil cooling rule hands the coil what the exchanger's own
        /// state delivers: coolth recovery when the intake is warmer than the extract, bypass when the bypass
        /// conditions hold, recovery below the bypass intake limit - then the drop, never below the limit.
        /// </summary>
        [Theory]
        [InlineData(30.0, 24.0, (0.8576 * 24.0) + (0.1424 * 30.0) - 8.245)]
        [InlineData(24.0, 25.0, 24.0 - 8.245)]
        [InlineData(20.0, 23.0, 13.0)]
        [InlineData(10.0, 23.0, 13.0)]
        public void ARoomStatExchangerThenCoilStrategy_CoolsWhatTheExchangerDelivers(double intake_C, double extract_C, double expected_C)
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.CoolingActivationSignal = CoolingActivationSignal.RoomTemperature;
            ventilationUnitOperatingStrategy.MinimumElevatedAirFlow_Lps = 60.0;
            ventilationUnitOperatingStrategy.MaximumElevatedAirFlow_Lps = 120.0;
            ventilationUnitOperatingStrategy.CoolingSupplyTemperatureRule = ExchangerThenCoilRule();

            double supply_C = ventilationUnitOperatingStrategy.SupplyTemperature(intake_C, extract_C, 23.0, 30.0, null, out VentilationUnitOperatingMode mode, out double flow_Lps);

            Assert.Equal(VentilationUnitOperatingMode.Cooling, mode);
            Assert.Equal(80.0, flow_Lps, tolerance);
            Assert.Equal(expected_C, supply_C, tolerance);
        }

        /// <summary>
        /// A stated default elevated airflow is a catalogue figure: it round-trips, is held to the stated range,
        /// and does not resolve the strategy - a dwelling's own figure is still what resolves it.
        /// </summary>
        [Fact]
        public void ADefaultElevatedAirflow_RoundTrips_IsHeldToTheRange_AndDoesNotResolve()
        {
            VentilationUnitOperatingStrategy ventilationUnitOperatingStrategy = Strategy();
            ventilationUnitOperatingStrategy.ElevatedAirFlow_Lps = double.NaN;
            ventilationUnitOperatingStrategy.DefaultElevatedAirFlow_Lps = 80.0;

            Assert.Null(ventilationUnitOperatingStrategy.TemplateRefusal());
            Assert.False(ventilationUnitOperatingStrategy.IsResolved);
            Assert.Equal(80.0, new VentilationUnitOperatingStrategy(ventilationUnitOperatingStrategy.ToJsonObject()).DefaultElevatedAirFlow_Lps);
            Assert.Equal(80.0, new VentilationUnitOperatingStrategy(ventilationUnitOperatingStrategy).DefaultElevatedAirFlow_Lps);

            ventilationUnitOperatingStrategy.DefaultElevatedAirFlow_Lps = 95.0;
            Assert.NotNull(ventilationUnitOperatingStrategy.TemplateRefusal());

            ventilationUnitOperatingStrategy.DefaultElevatedAirFlow_Lps = 0.0;
            Assert.NotNull(ventilationUnitOperatingStrategy.TemplateRefusal());
        }

        // =================================================================================================
        // Fixtures
        // =================================================================================================

        /// <summary>
        /// A fixture exchanger-then-coil rule with the figures of the Nuaire reply of 24 Sep 2026: extract fraction
        /// 87.96 / 85.76 / 83.56 / 81.36 %, coil drop 9.265 / 8.745 / 8.225 / 7.705 K and fan rise 0.3 / 0.5 / 0.8 /
        /// 1.1 K at 60 / 80 / 100 / 120 l/s, 13 &#176;C limit, refusing outside.
        /// </summary>
        private static SupplyTemperatureRule ExchangerThenCoilRule()
        {
            return SupplyTemperatureRule.ExchangerThenCoil([60.0, 80.0, 100.0, 120.0], [0.8796, 0.8576, 0.8356, 0.8136], [9.265, 8.745, 8.225, 7.705], [0.3, 0.5, 0.8, 1.1], 13.0);
        }

        /// <summary>A fixture intake-offset rule: 15 / 14 / 13 K at 70 / 80 / 90 l/s, no floor, refusing outside.</summary>
        private static SupplyTemperatureRule IntakeOffsetRule()
        {
            return SupplyTemperatureRule.IntakeOffset([70.0, 80.0, 90.0], [15.0, 14.0, 13.0]);
        }

        /// <summary>
        /// A fixture strategy in the shape a hybrid cooling unit's guidance takes: cooling on the extract
        /// temperature alone above 22 &#176;C (adjustable to 25 &#176;C), bypass while the intake is above
        /// 12 &#176;C and the extract is above both the intake and 18 &#176;C, and recovery for everything
        /// else.
        /// </summary>
        private static VentilationUnitOperatingStrategy Strategy()
        {
            return new VentilationUnitOperatingStrategy
            {
                Source = source_Fixture,
                CoolingActivationTemperature_C = 22.0,
                MinimumCoolingActivationTemperature_C = 22.0,
                MaximumCoolingActivationTemperature_C = 25.0,
                BypassMinimumIntakeTemperature_C = 12.0,
                BypassMinimumExtractTemperature_C = 18.0,
                ElevatedAirFlow_Lps = 80.0,
                MinimumElevatedAirFlow_Lps = 70.0,
                MaximumElevatedAirFlow_Lps = 90.0,
                SummerBypassSupplyTemperatureRule = SupplyTemperatureRule.OutdoorAir(),
                HeatCoolthRecoverySupplyTemperatureRule = SupplyTemperatureRule.LinearBlend(0.8),
                CoolingSupplyTemperatureRule = SupplyTemperatureRule.PerformanceTable(16.0),
            };
        }

        /// <summary>
        /// A fixture supply-air temperature table over intake temperature, extract temperature and airflow -
        /// the same three axes a real cooling module's selection table is published over, with invented
        /// figures.
        /// </summary>
        private static VentilationUnitPerformanceTable Table()
        {
            double[] intakeTemperatures_C = [25.0, 30.0];
            double[] extractTemperatures_C = [23.0, 24.0];
            double[] airFlowRates_Lps = [70.0, 80.0];

            //Index order is intake-major, then extract, then airflow.
            double[] supplyTemperatures_C =
            [
                14.0, 14.0,
                15.0, 15.0,
                17.0, 17.0,
                18.0, 18.0,
            ];

            return new VentilationUnitPerformanceTable(
                [
                    new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_ExternalDryBulbTemperature, VentilationUnitPerformanceAxis.Unit_DegreesCelsius, intakeTemperatures_C),
                    new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_EnteringDryBulbTemperature, VentilationUnitPerformanceAxis.Unit_DegreesCelsius, extractTemperatures_C),
                    new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_AirFlowRate, VentilationUnitPerformanceAxis.Unit_LitresPerSecond, airFlowRates_Lps),
                ],
                [
                    new VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput.Name_SupplyAirTemperature, VentilationUnitPerformanceOutput.Unit_DegreesCelsius, supplyTemperatures_C),
                ]);
        }
    }
}
