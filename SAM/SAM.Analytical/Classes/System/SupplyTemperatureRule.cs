// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Globalization;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// What air temperature a ventilation unit delivers into the dwelling in one of its operating modes,
    /// as its manufacturer states it - a rule expressed as data rather than as code.
    /// <para>
    /// <b>The quantity is the package supply temperature.</b> It is the air leaving the whole unit: every
    /// component inside the casing, in the arrangement the manufacturer built, has already acted on it.
    /// That is the number a manufacturer publishes and the number that can be observed downstream of the
    /// unit in a simulation, which is why it - and not any internal component property - is what this
    /// vocabulary carries and what an implementation is held to.
    /// </para>
    /// <para>
    /// <b><see cref="ExtractFraction"/> is not a certified heat-recovery efficiency and must never be
    /// written as one.</b> A manufacturer's simplified modelling guidance may state that its unit delivers,
    /// say, four fifths of the way from intake to extract temperature. A certified EN 13141-7 / SAP
    /// temperature ratio is a measured property of a specified test, on a stated basis, for a stated
    /// product configuration. The two answer different questions and are traceable to different documents,
    /// so a blend fraction stated here is carried here, is reported as manufacturer modelling guidance, and
    /// is never copied into <see cref="HeatRecoveryPerformance"/> - which exists for the certified figure
    /// and is what <c>Query.VentilationUnitOperatingParameters</c> requires. Numerical similarity between
    /// the two is not evidence that either is the other.
    /// </para>
    /// <para>
    /// <b>A rule of type <see cref="SupplyTemperatureRuleType.PerformanceTable"/> carries no table.</b> The
    /// product's published data already lives on its <see cref="VentilationUnitTemplate"/>; a copy here
    /// would be a second place for it to be wrong. The rule says "read the published table" and whoever
    /// resolves the strategy hands it in - see <see cref="SupplyTemperature(double, double, double,
    /// VentilationUnitPerformanceTable)"/>.
    /// </para>
    /// </summary>
    public class SupplyTemperatureRule : IJSAMObject
    {
        private SupplyTemperatureRuleType supplyTemperatureRuleType = SupplyTemperatureRuleType.Undefined;
        private double extractFraction = double.NaN;
        private double minimumSupplyTemperature_C = double.NaN;
        private PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.ClampToDomain;
        private bool performanceDomainPolicy_Refused = false;
        private double[] airFlowRates_Lps = null;
        private double[] intakeOffsets_K = null;
        private double[] extractFractions = null;
        private double[] coilTemperatureDrops_K = null;
        private double[] fanTemperatureRises_K = null;

        public SupplyTemperatureRule()
        {
        }

        /// <summary>The rule for a mode that delivers intake air unchanged.</summary>
        public static SupplyTemperatureRule OutdoorAir()
        {
            return new SupplyTemperatureRule { supplyTemperatureRuleType = SupplyTemperatureRuleType.OutdoorAir };
        }

        /// <summary>
        /// The rule for a mode that delivers a stated blend of extract and intake air -
        /// <c>extractFraction * extract + (1 - extractFraction) * intake</c>. Read the type remarks before
        /// supplying a fraction taken from a certified document rather than from modelling guidance.
        /// </summary>
        public static SupplyTemperatureRule LinearBlend(double extractFraction)
        {
            return new SupplyTemperatureRule
            {
                supplyTemperatureRuleType = SupplyTemperatureRuleType.LinearBlend,
                extractFraction = extractFraction,
            };
        }

        /// <summary>
        /// The rule for a mode whose supply temperature the manufacturer publishes as a table, optionally
        /// with a stated lower limit.
        /// </summary>
        /// <param name="minimumSupplyTemperature_C">
        /// The lowest supply temperature [&#176;C] the manufacturer's guidance allows, or
        /// <see cref="double.NaN"/> where it states none. Applied to the table's answer, never to the
        /// table's own published cells - a published figure stays exactly what was published, and a limit
        /// stated by guidance is applied by the guidance route that states it.
        /// </param>
        /// <param name="performanceDomainPolicy">
        /// What the lookup does outside the published grid. Defaults to
        /// <see cref="PerformanceDomainPolicy.ClampToDomain"/> - holding at the published edges, which is
        /// what the already-accepted cooling route does, and is necessary because a cooling table published
        /// over summer design conditions is asked about every hour of the year.
        /// </param>
        public static SupplyTemperatureRule PerformanceTable(double minimumSupplyTemperature_C = double.NaN, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.ClampToDomain)
        {
            return new SupplyTemperatureRule
            {
                supplyTemperatureRuleType = SupplyTemperatureRuleType.PerformanceTable,
                minimumSupplyTemperature_C = minimumSupplyTemperature_C,
                performanceDomainPolicy = performanceDomainPolicy,
            };
        }

        /// <summary>
        /// The rule for a mode that delivers intake air less an offset stated per airflow -
        /// <c>intake - X(airflow)</c> - with X interpolated linearly between the stated airflows.
        /// </summary>
        /// <param name="airFlowRates_Lps">The airflows [l/s] at which the offset is stated, strictly increasing.</param>
        /// <param name="intakeOffsets_K">The offset X [K] at each of those airflows; positive cools.</param>
        /// <param name="minimumSupplyTemperature_C">
        /// The lowest supply temperature [&#176;C] the guidance allows, or <see cref="double.NaN"/> where it
        /// states none.
        /// </param>
        /// <param name="performanceDomainPolicy">
        /// What the lookup does at an airflow outside the stated ones. Defaults to
        /// <see cref="PerformanceDomainPolicy.Refuse"/>: an offset nobody stated is not a figure to hold at
        /// the nearest edge without saying so. Whichever policy applies, the use is reportable through
        /// <see cref="AirFlowDomainCondition(double)"/>.
        /// </param>
        public static SupplyTemperatureRule IntakeOffset(double[] airFlowRates_Lps, double[] intakeOffsets_K, double minimumSupplyTemperature_C = double.NaN, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
        {
            return new SupplyTemperatureRule
            {
                supplyTemperatureRuleType = SupplyTemperatureRuleType.IntakeOffset,
                airFlowRates_Lps = airFlowRates_Lps?.Clone() as double[],
                intakeOffsets_K = intakeOffsets_K?.Clone() as double[],
                minimumSupplyTemperature_C = minimumSupplyTemperature_C,
                performanceDomainPolicy = performanceDomainPolicy,
            };
        }

        /// <summary>
        /// The rule for a mode in which the unit's exchanger acts first and a cooling coil then lowers what
        /// leaves it - the stated component process behind an <see cref="IntakeOffset"/> figure. Every figure
        /// is stated per airflow and interpolated linearly between the stated airflows.
        /// <para>
        /// The exchanger delivers intake air while bypassed, otherwise
        /// <c>extractFraction * extract + (1 - extractFraction) * intake</c>. The coil then lowers that by its
        /// stated drop, less the stated temperature rise of the fan motor heat the air picks up on the way,
        /// and never below <paramref name="minimumSupplyTemperature_C"/>. <b>A coil only cools</b>: where the
        /// exchanger already delivers air at or below the limit, the coil does nothing and that air is what
        /// the unit supplies - the limit bounds what the coil produces, it is not a heater.
        /// </para>
        /// <para>
        /// The fraction is manufacturer modelling guidance, <b>not a certified heat-recovery efficiency</b> -
        /// read the type remarks.
        /// </para>
        /// </summary>
        /// <param name="airFlowRates_Lps">The airflows [l/s] at which the figures are stated, strictly increasing.</param>
        /// <param name="extractFractions">The exchanger's extract fraction [-] at each airflow while it is not bypassed.</param>
        /// <param name="coilTemperatureDrops_K">The coil's temperature drop [K] at each airflow; positive cools.</param>
        /// <param name="fanTemperatureRises_K">The fan motor heat's temperature rise [K] at each airflow.</param>
        /// <param name="minimumSupplyTemperature_C">
        /// The lowest temperature [&#176;C] the coil delivers, or <see cref="double.NaN"/> where none is stated.
        /// </param>
        /// <param name="performanceDomainPolicy">
        /// What the lookup does at an airflow outside the stated ones. Defaults to
        /// <see cref="PerformanceDomainPolicy.Refuse"/>, as for <see cref="IntakeOffset"/>.
        /// </param>
        public static SupplyTemperatureRule ExchangerThenCoil(double[] airFlowRates_Lps, double[] extractFractions, double[] coilTemperatureDrops_K, double[] fanTemperatureRises_K, double minimumSupplyTemperature_C = double.NaN, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
        {
            return new SupplyTemperatureRule
            {
                supplyTemperatureRuleType = SupplyTemperatureRuleType.ExchangerThenCoil,
                airFlowRates_Lps = airFlowRates_Lps?.Clone() as double[],
                extractFractions = extractFractions?.Clone() as double[],
                coilTemperatureDrops_K = coilTemperatureDrops_K?.Clone() as double[],
                fanTemperatureRises_K = fanTemperatureRises_K?.Clone() as double[],
                minimumSupplyTemperature_C = minimumSupplyTemperature_C,
                performanceDomainPolicy = performanceDomainPolicy,
            };
        }

        public SupplyTemperatureRule(SupplyTemperatureRule supplyTemperatureRule)
        {
            if (supplyTemperatureRule is not null)
            {
                supplyTemperatureRuleType = supplyTemperatureRule.supplyTemperatureRuleType;
                extractFraction = supplyTemperatureRule.extractFraction;
                minimumSupplyTemperature_C = supplyTemperatureRule.minimumSupplyTemperature_C;
                performanceDomainPolicy = supplyTemperatureRule.performanceDomainPolicy;
                performanceDomainPolicy_Refused = supplyTemperatureRule.performanceDomainPolicy_Refused;
                airFlowRates_Lps = supplyTemperatureRule.airFlowRates_Lps?.Clone() as double[];
                intakeOffsets_K = supplyTemperatureRule.intakeOffsets_K?.Clone() as double[];
                extractFractions = supplyTemperatureRule.extractFractions?.Clone() as double[];
                coilTemperatureDrops_K = supplyTemperatureRule.coilTemperatureDrops_K?.Clone() as double[];
                fanTemperatureRises_K = supplyTemperatureRule.fanTemperatureRises_K?.Clone() as double[];
            }
        }

        public SupplyTemperatureRule(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>How this mode's supply temperature is stated.</summary>
        public SupplyTemperatureRuleType SupplyTemperatureRuleType
        {
            get
            {
                return supplyTemperatureRuleType;
            }
        }

        /// <summary>
        /// The share [-] of the way from intake to extract temperature this mode delivers, for a
        /// <see cref="SupplyTemperatureRuleType.LinearBlend"/> rule; <see cref="double.NaN"/> otherwise.
        /// <b>Not a certified heat-recovery efficiency</b> - see the type remarks.
        /// </summary>
        public double ExtractFraction
        {
            get
            {
                return extractFraction;
            }
        }

        /// <summary>
        /// The lowest supply temperature [&#176;C] the rule allows, or <see cref="double.NaN"/> where none
        /// is stated. Applied to what the rule computes, not to any published cell.
        /// </summary>
        public double MinimumSupplyTemperature_C
        {
            get
            {
                return minimumSupplyTemperature_C;
            }
        }

        /// <summary>
        /// The airflows [l/s] at which an <see cref="SupplyTemperatureRuleType.IntakeOffset"/> rule states
        /// its offset; null otherwise. A copy - the rule cannot be edited through it.
        /// </summary>
        public double[] AirFlowRates_Lps
        {
            get
            {
                return airFlowRates_Lps?.Clone() as double[];
            }
        }

        /// <summary>
        /// The offset X [K] at each of <see cref="AirFlowRates_Lps"/>, for an
        /// <see cref="SupplyTemperatureRuleType.IntakeOffset"/> rule; null otherwise. A copy.
        /// </summary>
        public double[] IntakeOffsets_K
        {
            get
            {
                return intakeOffsets_K?.Clone() as double[];
            }
        }

        /// <summary>
        /// The exchanger's extract fraction [-] at each of <see cref="AirFlowRates_Lps"/>, for an
        /// <see cref="SupplyTemperatureRuleType.ExchangerThenCoil"/> rule; null otherwise. A copy.
        /// <b>Not a certified heat-recovery efficiency</b> - see the type remarks.
        /// </summary>
        public double[] ExtractFractions
        {
            get
            {
                return extractFractions?.Clone() as double[];
            }
        }

        /// <summary>
        /// The coil's temperature drop [K] at each of <see cref="AirFlowRates_Lps"/>, for an
        /// <see cref="SupplyTemperatureRuleType.ExchangerThenCoil"/> rule; null otherwise. A copy.
        /// </summary>
        public double[] CoilTemperatureDrops_K
        {
            get
            {
                return coilTemperatureDrops_K?.Clone() as double[];
            }
        }

        /// <summary>
        /// The fan motor heat's temperature rise [K] at each of <see cref="AirFlowRates_Lps"/>, for an
        /// <see cref="SupplyTemperatureRuleType.ExchangerThenCoil"/> rule; null otherwise. A copy.
        /// </summary>
        public double[] FanTemperatureRises_K
        {
            get
            {
                return fanTemperatureRises_K?.Clone() as double[];
            }
        }

        /// <summary>What a table lookup does outside the published grid.</summary>
        public PerformanceDomainPolicy PerformanceDomainPolicy
        {
            get
            {
                return performanceDomainPolicy;
            }
        }

        /// <summary>
        /// Why this rule cannot be used, in words, or null where it can be.
        /// <para>
        /// Every rule is a refusal rather than a repair: a fraction outside the unit interval, a
        /// non-finite limit or an unrecognised policy name is a transcription mistake, and the one
        /// direction a transcription mistake must never take is "close enough".
        /// </para>
        /// </summary>
        public string Refusal()
        {
            if (performanceDomainPolicy_Refused)
            {
                return "states a performance domain policy that is not one of the stated names.";
            }

            switch (supplyTemperatureRuleType)
            {
                case SupplyTemperatureRuleType.Undefined:
                    return "states no supply-temperature rule.";

                case SupplyTemperatureRuleType.OutdoorAir:
                    break;

                case SupplyTemperatureRuleType.LinearBlend:
                    if (double.IsNaN(extractFraction) || double.IsInfinity(extractFraction))
                    {
                        return "states a blend of extract and intake air without stating the extract fraction.";
                    }

                    if (extractFraction < 0 || extractFraction > 1)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "states an extract fraction of {0}; a blend of two air streams lies between 0 and 1.", extractFraction);
                    }

                    break;

                case SupplyTemperatureRuleType.PerformanceTable:
                    break;

                case SupplyTemperatureRuleType.IntakeOffset:
                    string refusal_Offset = IntakeOffsetRefusal();
                    if (refusal_Offset is not null)
                    {
                        return refusal_Offset;
                    }

                    break;

                case SupplyTemperatureRuleType.ExchangerThenCoil:
                    string refusal_Coil = ExchangerThenCoilRefusal();
                    if (refusal_Coil is not null)
                    {
                        return refusal_Coil;
                    }

                    break;

                default:
                    return string.Format("states the unrecognised supply-temperature rule '{0}'.", supplyTemperatureRuleType);
            }

            if (double.IsInfinity(minimumSupplyTemperature_C))
            {
                return "states a minimum supply temperature that is not a finite number.";
            }

            return null;
        }

        /// <summary>
        /// The package supply temperature [&#176;C] this rule states, for one hour's intake and extract air
        /// temperatures and the airflow the unit is moving.
        /// </summary>
        /// <param name="intakeTemperature_C">The outdoor / intake air temperature [&#176;C] at the unit.</param>
        /// <param name="extractTemperature_C">The extract / return air temperature [&#176;C] at the unit.</param>
        /// <param name="airFlowRate_Lps">The airflow [l/s] the unit is moving in this mode.</param>
        /// <param name="ventilationUnitPerformanceTable">
        /// The product's published table, required only by a
        /// <see cref="SupplyTemperatureRuleType.PerformanceTable"/> rule and ignored by the others.
        /// </param>
        /// <returns>
        /// The supply temperature, or <see cref="double.NaN"/> where the rule refuses, an input is not a
        /// finite number, or the table cannot answer - and always for an
        /// <see cref="SupplyTemperatureRuleType.ExchangerThenCoil"/> rule, whose answer depends on whether the
        /// exchanger is bypassed: use <see cref="SupplyTemperature(double, double, double, bool)"/>.
        /// </returns>
        public double SupplyTemperature(double intakeTemperature_C, double extractTemperature_C, double airFlowRate_Lps, VentilationUnitPerformanceTable ventilationUnitPerformanceTable = null)
        {
            if (Refusal() is not null || !IsFinite(intakeTemperature_C) || !IsFinite(extractTemperature_C))
            {
                return double.NaN;
            }

            double result;

            switch (supplyTemperatureRuleType)
            {
                case SupplyTemperatureRuleType.OutdoorAir:
                    result = intakeTemperature_C;
                    break;

                case SupplyTemperatureRuleType.LinearBlend:
                    result = (extractFraction * extractTemperature_C) + ((1 - extractFraction) * intakeTemperature_C);
                    break;

                case SupplyTemperatureRuleType.PerformanceTable:
                    if (ventilationUnitPerformanceTable is null || !IsFinite(airFlowRate_Lps))
                    {
                        return double.NaN;
                    }

                    result = ventilationUnitPerformanceTable.Value(
                        VentilationUnitPerformanceOutput.Name_SupplyAirTemperature,
                        TableArguments(ventilationUnitPerformanceTable, intakeTemperature_C, extractTemperature_C, airFlowRate_Lps),
                        performanceDomainPolicy);

                    break;

                case SupplyTemperatureRuleType.IntakeOffset:
                    double intakeOffset_K = IntakeOffset_K(airFlowRate_Lps);
                    if (!IsFinite(intakeOffset_K))
                    {
                        return double.NaN;
                    }

                    result = intakeTemperature_C - intakeOffset_K;
                    break;

                default:
                    return double.NaN;
            }

            if (!IsFinite(result))
            {
                return double.NaN;
            }

            //A stated lower limit is applied to what the rule computes. It never reaches back into a
            //published cell: the published table stays exactly what the manufacturer printed, and a limit
            //its guidance states is applied by the guidance route that states it.
            return double.IsNaN(minimumSupplyTemperature_C) ? result : System.Math.Max(result, minimumSupplyTemperature_C);
        }

        /// <summary>
        /// The offset X [K] an <see cref="SupplyTemperatureRuleType.IntakeOffset"/> rule applies at an
        /// airflow: linear between the stated airflows; outside them held at the nearest stated one under
        /// <see cref="PerformanceDomainPolicy.ClampToDomain"/> and <see cref="double.NaN"/> under
        /// <see cref="PerformanceDomainPolicy.Refuse"/>. <see cref="double.NaN"/> for any other rule or a
        /// rule that refuses.
        /// </summary>
        public double IntakeOffset_K(double airFlowRate_Lps)
        {
            if (supplyTemperatureRuleType != SupplyTemperatureRuleType.IntakeOffset || Refusal() is not null)
            {
                return double.NaN;
            }

            return Interpolate(intakeOffsets_K, airFlowRate_Lps);
        }

        /// <summary>
        /// The package supply temperature [&#176;C] this rule states for one hour, given whether the unit's
        /// exchanger is bypassed in that hour. For an <see cref="SupplyTemperatureRuleType.ExchangerThenCoil"/>
        /// rule the exchanger state decides what reaches the coil; every other rule ignores it and answers as
        /// <see cref="SupplyTemperature(double, double, double, VentilationUnitPerformanceTable)"/> does
        /// without a table.
        /// </summary>
        /// <param name="intakeTemperature_C">The outdoor / intake air temperature [&#176;C] at the unit.</param>
        /// <param name="extractTemperature_C">The extract / return air temperature [&#176;C] at the unit.</param>
        /// <param name="airFlowRate_Lps">The airflow [l/s] the unit is moving in this mode.</param>
        /// <param name="exchangerBypassed">Whether the exchanger is bypassed in this hour.</param>
        public double SupplyTemperature(double intakeTemperature_C, double extractTemperature_C, double airFlowRate_Lps, bool exchangerBypassed)
        {
            if (supplyTemperatureRuleType != SupplyTemperatureRuleType.ExchangerThenCoil)
            {
                return SupplyTemperature(intakeTemperature_C, extractTemperature_C, airFlowRate_Lps);
            }

            if (Refusal() is not null || !IsFinite(intakeTemperature_C) || !IsFinite(extractTemperature_C))
            {
                return double.NaN;
            }

            double coilDrop_K = CoilNetTemperatureDrop_K(airFlowRate_Lps);
            double extractFraction_Airflow = exchangerBypassed ? 0.0 : ExchangerExtractFraction(airFlowRate_Lps);
            if (!IsFinite(coilDrop_K) || !IsFinite(extractFraction_Airflow))
            {
                return double.NaN;
            }

            double exchangerLeaving_C = (extractFraction_Airflow * extractTemperature_C) + ((1 - extractFraction_Airflow) * intakeTemperature_C);
            double result = exchangerLeaving_C - coilDrop_K;

            //The limit bounds what the coil produces. A coil only cools, so air that already leaves the exchanger
            //at or below the limit passes through unchanged rather than being warmed up to it.
            if (!double.IsNaN(minimumSupplyTemperature_C))
            {
                result = System.Math.Min(exchangerLeaving_C, System.Math.Max(result, minimumSupplyTemperature_C));
            }

            return result;
        }

        /// <summary>
        /// The exchanger's extract fraction [-] an <see cref="SupplyTemperatureRuleType.ExchangerThenCoil"/> rule
        /// applies at an airflow while the exchanger is not bypassed - linear between the stated airflows,
        /// outside them per <see cref="PerformanceDomainPolicy"/>. <see cref="double.NaN"/> for any other rule
        /// or a rule that refuses.
        /// </summary>
        public double ExchangerExtractFraction(double airFlowRate_Lps)
        {
            if (supplyTemperatureRuleType != SupplyTemperatureRuleType.ExchangerThenCoil || Refusal() is not null)
            {
                return double.NaN;
            }

            return Interpolate(extractFractions, airFlowRate_Lps);
        }

        /// <summary>
        /// What an <see cref="SupplyTemperatureRuleType.ExchangerThenCoil"/> rule's coil takes off the air at an
        /// airflow [K]: the coil drop less the fan motor heat's rise, each interpolated as
        /// <see cref="ExchangerExtractFraction(double)"/> is. <see cref="double.NaN"/> for any other rule or a
        /// rule that refuses.
        /// </summary>
        public double CoilNetTemperatureDrop_K(double airFlowRate_Lps)
        {
            if (supplyTemperatureRuleType != SupplyTemperatureRuleType.ExchangerThenCoil || Refusal() is not null)
            {
                return double.NaN;
            }

            return Interpolate(coilTemperatureDrops_K, airFlowRate_Lps) - Interpolate(fanTemperatureRises_K, airFlowRate_Lps);
        }

        /// <summary>
        /// One figure stated per airflow, at an airflow: linear between the stated airflows; outside them held at
        /// the nearest stated one under <see cref="PerformanceDomainPolicy.ClampToDomain"/> and
        /// <see cref="double.NaN"/> otherwise.
        /// </summary>
        private double Interpolate(double[] values, double airFlowRate_Lps)
        {
            if (!IsFinite(airFlowRate_Lps))
            {
                return double.NaN;
            }

            int count = airFlowRates_Lps.Length;

            if (airFlowRate_Lps < airFlowRates_Lps[0] || airFlowRate_Lps > airFlowRates_Lps[count - 1])
            {
                if (performanceDomainPolicy != PerformanceDomainPolicy.ClampToDomain)
                {
                    return double.NaN;
                }

                return airFlowRate_Lps < airFlowRates_Lps[0] ? values[0] : values[count - 1];
            }

            for (int i = 0; i < count - 1; i++)
            {
                if (airFlowRate_Lps <= airFlowRates_Lps[i + 1])
                {
                    double fraction = (airFlowRate_Lps - airFlowRates_Lps[i]) / (airFlowRates_Lps[i + 1] - airFlowRates_Lps[i]);
                    return values[i] + (fraction * (values[i + 1] - values[i]));
                }
            }

            return values[count - 1];
        }

        /// <summary>
        /// Why an airflow is outside what an <see cref="SupplyTemperatureRuleType.IntakeOffset"/> rule
        /// states, in words, or null where the offset at that airflow is stated or interpolated between
        /// stated airflows (and for every other rule type).
        /// <para>
        /// Reported whatever the <see cref="PerformanceDomainPolicy"/>: clamping answers "what number", not
        /// "was that number stated".
        /// </para>
        /// </summary>
        public string AirFlowDomainCondition(double airFlowRate_Lps)
        {
            bool isOffset = supplyTemperatureRuleType == SupplyTemperatureRuleType.IntakeOffset;
            if (isOffset ? IntakeOffsetRefusal() is not null : (supplyTemperatureRuleType != SupplyTemperatureRuleType.ExchangerThenCoil || ExchangerThenCoilRefusal() is not null))
            {
                return null;
            }

            if (!IsFinite(airFlowRate_Lps))
            {
                return "the airflow is not a finite number.";
            }

            double minimum = airFlowRates_Lps[0];
            double maximum = airFlowRates_Lps[airFlowRates_Lps.Length - 1];

            if (airFlowRate_Lps >= minimum && airFlowRate_Lps <= maximum)
            {
                return null;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.###} l/s is outside the {1:0.###} to {2:0.###} l/s at which the {3} stated ({4}).",
                airFlowRate_Lps,
                minimum,
                maximum,
                isOffset ? "intake offset is" : "exchanger and coil figures are",
                performanceDomainPolicy == PerformanceDomainPolicy.ClampToDomain
                    ? (isOffset ? "held at the nearest stated offset" : "held at the nearest stated figures")
                    : (isOffset ? "no offset is given" : "no figure is given"));
        }

        private string ExchangerThenCoilRefusal()
        {
            if (airFlowRates_Lps is null || extractFractions is null || coilTemperatureDrops_K is null || fanTemperatureRises_K is null || airFlowRates_Lps.Length == 0)
            {
                return "states an exchanger and coil without stating the airflows, extract fractions, coil drops and fan rises they apply at.";
            }

            int count = airFlowRates_Lps.Length;
            if (extractFractions.Length != count || coilTemperatureDrops_K.Length != count || fanTemperatureRises_K.Length != count)
            {
                return string.Format("states {0} airflow(s) but {1} extract fraction(s), {2} coil drop(s) and {3} fan rise(s).", count, extractFractions.Length, coilTemperatureDrops_K.Length, fanTemperatureRises_K.Length);
            }

            for (int i = 0; i < count; i++)
            {
                if (!IsFinite(airFlowRates_Lps[i]) || airFlowRates_Lps[i] <= 0 || !IsFinite(extractFractions[i]) || !IsFinite(coilTemperatureDrops_K[i]) || !IsFinite(fanTemperatureRises_K[i]))
                {
                    return "states an exchanger and coil figure that is not a finite number, or an airflow that is not positive.";
                }

                if (i > 0 && airFlowRates_Lps[i] <= airFlowRates_Lps[i - 1])
                {
                    return "states exchanger and coil airflows that are not strictly increasing.";
                }

                if (extractFractions[i] < 0 || extractFractions[i] > 1)
                {
                    return string.Format(CultureInfo.InvariantCulture, "states an exchanger extract fraction of {0}; a blend of two air streams lies between 0 and 1.", extractFractions[i]);
                }

                if (fanTemperatureRises_K[i] < 0 || coilTemperatureDrops_K[i] < fanTemperatureRises_K[i])
                {
                    return string.Format(CultureInfo.InvariantCulture, "states a coil drop of {0} K after a fan rise of {1} K at {2} l/s; a cooling coil states at least the fan heat it follows, and a fan does not cool.", coilTemperatureDrops_K[i], fanTemperatureRises_K[i], airFlowRates_Lps[i]);
                }
            }

            return null;
        }

        private string IntakeOffsetRefusal()
        {
            if (airFlowRates_Lps is null || intakeOffsets_K is null || airFlowRates_Lps.Length == 0)
            {
                return "states an intake offset without stating the airflows and offsets it applies at.";
            }

            if (airFlowRates_Lps.Length != intakeOffsets_K.Length)
            {
                return string.Format("states {0} airflow(s) but {1} intake offset(s).", airFlowRates_Lps.Length, intakeOffsets_K.Length);
            }

            for (int i = 0; i < airFlowRates_Lps.Length; i++)
            {
                if (!IsFinite(airFlowRates_Lps[i]) || airFlowRates_Lps[i] <= 0 || !IsFinite(intakeOffsets_K[i]))
                {
                    return "states an intake offset airflow or offset that is not a finite number, or an airflow that is not positive.";
                }

                if (i > 0 && airFlowRates_Lps[i] <= airFlowRates_Lps[i - 1])
                {
                    return "states intake offset airflows that are not strictly increasing.";
                }
            }

            return null;
        }

        /// <summary>
        /// The arguments of a published supply-air temperature table, in the order that table states its
        /// axes.
        /// <para>
        /// <b>By axis name, never by position.</b> The table's axes are its own; reading them positionally
        /// would silently swap an intake temperature for an extract temperature the first time a catalogue
        /// wrote them the other way round.
        /// </para>
        /// </summary>
        private static double[] TableArguments(VentilationUnitPerformanceTable ventilationUnitPerformanceTable, double intakeTemperature_C, double extractTemperature_C, double airFlowRate_Lps)
        {
            double[] result = new double[ventilationUnitPerformanceTable.AxisCount];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = double.NaN;
            }

            Set(VentilationUnitPerformanceAxis.Name_ExternalDryBulbTemperature, intakeTemperature_C);
            Set(VentilationUnitPerformanceAxis.Name_EnteringDryBulbTemperature, extractTemperature_C);
            Set(VentilationUnitPerformanceAxis.Name_AirFlowRate, airFlowRate_Lps);

            return result;

            void Set(string name, double value)
            {
                int index = ventilationUnitPerformanceTable.AxisIndex(name);
                if (index >= 0 && index < result.Length)
                {
                    result[index] = value;
                }
            }
        }

        public override string ToString()
        {
            string result;

            switch (supplyTemperatureRuleType)
            {
                case SupplyTemperatureRuleType.OutdoorAir:
                    result = "intake air";
                    break;

                case SupplyTemperatureRuleType.LinearBlend:
                    result = string.Format(CultureInfo.InvariantCulture, "{0:0.###} x extract + {1:0.###} x intake", extractFraction, 1 - extractFraction);
                    break;

                case SupplyTemperatureRuleType.PerformanceTable:
                    result = string.Format("published table ({0} outside)", performanceDomainPolicy);
                    break;

                case SupplyTemperatureRuleType.IntakeOffset:
                    if (IntakeOffsetRefusal() is not null)
                    {
                        return "Invalid SupplyTemperatureRule";
                    }

                    string[] points = new string[airFlowRates_Lps.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = string.Format(CultureInfo.InvariantCulture, "{0:0.###}:{1:0.###}", airFlowRates_Lps[i], intakeOffsets_K[i]);
                    }

                    result = string.Format("intake - X(l/s:K {0}; {1} outside)", string.Join(" ", points), performanceDomainPolicy);
                    break;

                case SupplyTemperatureRuleType.ExchangerThenCoil:
                    if (ExchangerThenCoilRefusal() is not null)
                    {
                        return "Invalid SupplyTemperatureRule";
                    }

                    string[] figures = new string[airFlowRates_Lps.Length];
                    for (int i = 0; i < figures.Length; i++)
                    {
                        figures[i] = string.Format(CultureInfo.InvariantCulture, "{0:0.###}:{1:0.####}/{2:0.###}-{3:0.###}", airFlowRates_Lps[i], extractFractions[i], coilTemperatureDrops_K[i], fanTemperatureRises_K[i]);
                    }

                    result = string.Format("exchanger (bypass or fraction) then coil drop less fan rise (l/s:fraction/drop-rise {0}; {1} outside)", string.Join(" ", figures), performanceDomainPolicy);
                    break;

                default:
                    return "Invalid SupplyTemperatureRule";
            }

            return double.IsNaN(minimumSupplyTemperature_C)
                ? result
                : string.Format(CultureInfo.InvariantCulture, "{0}, not below {1:0.###} degC", result, minimumSupplyTemperature_C);
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            supplyTemperatureRuleType = Core.Query.Enum<SupplyTemperatureRuleType>(PerformanceJson.Text(jsonObject, "SupplyTemperatureRuleType"));
            extractFraction = PerformanceJson.Value(jsonObject, "ExtractFraction");
            minimumSupplyTemperature_C = PerformanceJson.Value(jsonObject, "MinimumSupplyTemperature_C");
            airFlowRates_Lps = PerformanceJson.Values(jsonObject, "AirFlowRates_Lps");
            intakeOffsets_K = PerformanceJson.Values(jsonObject, "IntakeOffsets_K");
            extractFractions = PerformanceJson.Values(jsonObject, "ExtractFractions");
            coilTemperatureDrops_K = PerformanceJson.Values(jsonObject, "CoilTemperatureDrops_K");
            fanTemperatureRises_K = PerformanceJson.Values(jsonObject, "FanTemperatureRises_K");

            //Absent reads as ClampToDomain, which is what a published table asked about conditions outside
            //its grid already does on the accepted cooling route. PRESENT but not one of the names is
            //refused rather than quietly clamped - see FlowFractionControlCurve, which takes the same
            //position for the same reason.
            performanceDomainPolicy = PerformanceDomainPolicy.ClampToDomain;
            performanceDomainPolicy_Refused = false;

            string text = PerformanceJson.Text(jsonObject, "PerformanceDomainPolicy");

            if (!string.IsNullOrWhiteSpace(text))
            {
                PerformanceDomainPolicy performanceDomainPolicy_Temp = Core.Query.Enum<PerformanceDomainPolicy>(text);

                if (performanceDomainPolicy_Temp == PerformanceDomainPolicy.Undefined)
                {
                    performanceDomainPolicy_Refused = true;
                }
                else
                {
                    performanceDomainPolicy = performanceDomainPolicy_Temp;
                }
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject result = new()
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["SupplyTemperatureRuleType"] = supplyTemperatureRuleType.ToString(),
            };

            PerformanceJson.SetValue(result, "ExtractFraction", extractFraction);
            PerformanceJson.SetValue(result, "MinimumSupplyTemperature_C", minimumSupplyTemperature_C);
            PerformanceJson.SetValues(result, "AirFlowRates_Lps", airFlowRates_Lps);
            PerformanceJson.SetValues(result, "IntakeOffsets_K", intakeOffsets_K);
            PerformanceJson.SetValues(result, "ExtractFractions", extractFractions);
            PerformanceJson.SetValues(result, "CoilTemperatureDrops_K", coilTemperatureDrops_K);
            PerformanceJson.SetValues(result, "FanTemperatureRises_K", fanTemperatureRises_K);

            result["PerformanceDomainPolicy"] = performanceDomainPolicy.ToString();

            return result;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
