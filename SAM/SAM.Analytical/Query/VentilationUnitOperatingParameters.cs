// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The catalogue template for the product an air handling unit has been selected to be, or null
        /// where the unit has no selection or the catalogue does not hold exactly one entry for it.
        /// <para>
        /// Identity only - the unit's <see cref="SelectedVentilationUnitReference"/> matched through
        /// <see cref="MatchingVentilationUnitTemplate"/>. No unit, space or product name takes part.
        /// </para>
        /// </summary>
        public static VentilationUnitTemplate SelectedVentilationUnitTemplate(this AirHandlingUnit airHandlingUnit, IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates)
        {
            return MatchingVentilationUnitTemplate(ventilationUnitTemplates, SelectedVentilationUnitReference(airHandlingUnit));
        }

        /// <summary>
        /// Resolves a selected product identity to what its certified data says at a design duty. See the
        /// template overload for the contract.
        /// <para>
        /// <b>The product is found by identity</b> - <see cref="VentilationUnitReference.Matches"/>, never a
        /// name - and a catalogue that does not hold it, or holds it twice, refuses both quantities. Nothing
        /// is reselected, and the answer cannot depend on the order the catalogue was read in.
        /// </para>
        /// </summary>
        public static VentilationUnitOperatingParameters VentilationUnitOperatingParameters(this IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates, VentilationUnitReference ventilationUnitReference, double supplyAirFlowRate_Lps, double extractAirFlowRate_Lps, double tolerance_Lps = 0.001)
        {
            if (ventilationUnitReference is null || !ventilationUnitReference.IsValid)
            {
                return RefusedOperatingParameters(ventilationUnitReference, supplyAirFlowRate_Lps, extractAirFlowRate_Lps, "No ventilation unit product is selected, so there is no manufacturer data to resolve. No product is chosen on the unit's behalf.");
            }

            VentilationUnitTemplate ventilationUnitTemplate = null;
            int count = 0;

            foreach (VentilationUnitTemplate ventilationUnitTemplate_Temp in ventilationUnitTemplates ?? [])
            {
                if (ventilationUnitTemplate_Temp?.VentilationUnitReference is null || !ventilationUnitReference.Matches(ventilationUnitTemplate_Temp.VentilationUnitReference))
                {
                    continue;
                }

                ventilationUnitTemplate = ventilationUnitTemplate_Temp;
                count++;
            }

            if (count == 0)
            {
                return RefusedOperatingParameters(ventilationUnitReference, supplyAirFlowRate_Lps, extractAirFlowRate_Lps, string.Format(
                    "The selected product '{0}' is not among the ventilation unit templates supplied, so its manufacturer data cannot be resolved. No other product is substituted.",
                    ventilationUnitReference));
            }

            if (count > 1)
            {
                return RefusedOperatingParameters(ventilationUnitReference, supplyAirFlowRate_Lps, extractAirFlowRate_Lps, string.Format(
                    "The ventilation unit templates supplied hold {0} entries for '{1}', so it has no single set of manufacturer data. Answering from any one of them would make the result depend on the order the catalogue was read in.",
                    count,
                    ventilationUnitReference));
            }

            return VentilationUnitOperatingParameters(ventilationUnitTemplate, supplyAirFlowRate_Lps, extractAirFlowRate_Lps, tolerance_Lps);
        }

        /// <summary>
        /// What one product's certified data says it does at a design duty: its sensible heat recovery
        /// efficiency and its specific fan power, each resolved or refused on its own.
        ///
        /// <para><b>Inputs</b></para>
        /// <para>
        /// A template - already resolved from the selected identity - and the supply and extract design
        /// airflow [l/s] of the one unit it is fitted as. <b>The design airflow is a lookup coordinate and
        /// nothing else.</b> It is read, never changed, and never replaced by the product's capacity:
        /// <see cref="VentilationUnitTemplate.MaximumSupplyFlowRate_Lps"/> is not read here at all. Whether
        /// the product can move the duty is <see cref="IsVentilationUnitSufficient"/>'s question, asked
        /// separately.
        /// </para>
        ///
        /// <para><b>Outputs</b></para>
        /// <para>
        /// Always an object, never null, so every refusal is reportable. Each quantity is the published
        /// figure at a published airflow, linearly interpolated between two published airflows, or held at
        /// the published edge only where the data itself states
        /// <see cref="PerformanceDomainPolicy.ClampToDomain"/> - flagged on the result when that happens.
        /// Nothing is extrapolated.
        /// </para>
        ///
        /// <para><b>Refusals</b></para>
        /// <para>
        /// Both quantities: an unusable tolerance; no template, or one naming no product or stating no
        /// source; a design airflow that is not a finite positive number; <b>an unbalanced duty</b> - both
        /// quantities are certified at one balanced airflow and neither says anything about a unit moving
        /// different amounts of air on each side, so no rule for that is assumed.
        /// </para>
        /// <para>
        /// Each quantity on its own: the template states none (<b>absent is not zero</b>, and no generic or
        /// system-template value takes its place); the data is malformed, has the wrong units, an
        /// out-of-range or non-finite value, a duplicated operating point, a non-airflow axis, no basis, no
        /// source, or an extrapolating policy; or the design airflow lies outside the published operating
        /// points and the data does not state a clamp.
        /// </para>
        ///
        /// <para><b>Mutation</b></para>
        /// <para>
        /// None. The template, its tables and the reference are only read; the result holds its own copy
        /// of the reference. The same inputs always give the same result.
        /// </para>
        /// </summary>
        /// <param name="ventilationUnitTemplate">The selected product's catalogue entry.</param>
        /// <param name="supplyAirFlowRate_Lps">The unit's supply design airflow [l/s].</param>
        /// <param name="extractAirFlowRate_Lps">The unit's extract design airflow [l/s].</param>
        /// <param name="tolerance_Lps">How far the two design airflows may differ and still be one balanced duty [l/s].</param>
        public static VentilationUnitOperatingParameters VentilationUnitOperatingParameters(this VentilationUnitTemplate ventilationUnitTemplate, double supplyAirFlowRate_Lps, double extractAirFlowRate_Lps, double tolerance_Lps = 0.001)
        {
            VentilationUnitReference ventilationUnitReference = ventilationUnitTemplate?.VentilationUnitReference;

            string reason = OperatingPointRefusal(ventilationUnitTemplate, supplyAirFlowRate_Lps, extractAirFlowRate_Lps, tolerance_Lps);
            if (reason is not null)
            {
                return RefusedOperatingParameters(ventilationUnitReference, supplyAirFlowRate_Lps, extractAirFlowRate_Lps, reason);
            }

            double airFlowRate_Lps = (supplyAirFlowRate_Lps + extractAirFlowRate_Lps) / 2;

            HeatRecoveryPerformance heatRecoveryPerformance = ventilationUnitTemplate.HeatRecoveryPerformance;
            double sensibleHeatRecoveryEfficiency = heatRecoveryPerformance is null ? double.NaN : heatRecoveryPerformance.SensibleHeatRecoveryEfficiency(airFlowRate_Lps);
            string heatRecoveryRefusal = AirFlowPerformanceRefusal(ventilationUnitReference, heatRecoveryPerformance, "heat recovery performance", "a zero efficiency", airFlowRate_Lps, sensibleHeatRecoveryEfficiency);

            FanPerformance fanPerformance = ventilationUnitTemplate.FanPerformance;
            double specificFanPower_WPerLps = fanPerformance is null ? double.NaN : fanPerformance.SpecificFanPower_WPerLps(airFlowRate_Lps);
            string fanPerformanceRefusal = AirFlowPerformanceRefusal(ventilationUnitReference, fanPerformance, "fan performance", "a zero specific fan power", airFlowRate_Lps, specificFanPower_WPerLps);

            return new VentilationUnitOperatingParameters(
                ventilationUnitReference,
                supplyAirFlowRate_Lps,
                extractAirFlowRate_Lps,
                airFlowRate_Lps,
                heatRecoveryRefusal is null ? sensibleHeatRecoveryEfficiency : double.NaN,
                heatRecoveryRefusal is null ? heatRecoveryPerformance.HeatRecoveryEfficiencyBasis : HeatRecoveryEfficiencyBasis.Undefined,
                heatRecoveryRefusal is null && !heatRecoveryPerformance.InDomain(airFlowRate_Lps),
                heatRecoveryRefusal,
                fanPerformanceRefusal is null ? specificFanPower_WPerLps : double.NaN,
                fanPerformanceRefusal is null ? fanPerformance.SpecificFanPowerBasis : SpecificFanPowerBasis.Undefined,
                fanPerformanceRefusal is null && !fanPerformance.InDomain(airFlowRate_Lps),
                fanPerformanceRefusal);
        }

        /// <summary>Why a duty cannot be looked up on a template at all, or null where it can.</summary>
        private static string OperatingPointRefusal(VentilationUnitTemplate ventilationUnitTemplate, double supplyAirFlowRate_Lps, double extractAirFlowRate_Lps, double tolerance_Lps)
        {
            //Before any comparison - an unusable tolerance would otherwise decide balance by accident.
            if (!IsValidFlowRateTolerance(tolerance_Lps))
            {
                return FlowRateToleranceRefusal(tolerance_Lps);
            }

            if (ventilationUnitTemplate is null)
            {
                return "No ventilation unit template was supplied, so there is no manufacturer data to resolve.";
            }

            if (!ventilationUnitTemplate.IsValid)
            {
                return string.Format(
                    "The ventilation unit template '{0}' names no product or states no source, so its figures cannot be traced to a published document and none of them is used.",
                    ventilationUnitTemplate.VentilationUnitReference?.ToString() ?? "-");
            }

            if (!IsPositiveFlowRate(supplyAirFlowRate_Lps) || !IsPositiveFlowRate(extractAirFlowRate_Lps))
            {
                return string.Format(
                    "A design duty of supply {0} l/s, extract {1} l/s cannot be looked up on '{2}' - both have to be finite positive numbers of litres per second, and there is no neutral duty to assume.",
                    supplyAirFlowRate_Lps,
                    extractAirFlowRate_Lps,
                    ventilationUnitTemplate.VentilationUnitReference);
            }

            if (System.Math.Abs(supplyAirFlowRate_Lps - extractAirFlowRate_Lps) > tolerance_Lps)
            {
                return string.Format(
                    "The design duty of '{0}' is unbalanced - supply {1:0.###} l/s, extract {2:0.###} l/s. Heat recovery efficiency and specific fan power are certified at one balanced airflow, so neither says anything about this duty and neither is applied. No rule for an unbalanced unit is assumed.",
                    ventilationUnitTemplate.VentilationUnitReference,
                    supplyAirFlowRate_Lps,
                    extractAirFlowRate_Lps);
            }

            return null;
        }

        /// <summary>Why one quantity cannot be resolved at an airflow, or null where the value read is usable.</summary>
        private static string AirFlowPerformanceRefusal(VentilationUnitReference ventilationUnitReference, VentilationUnitAirFlowPerformance ventilationUnitAirFlowPerformance, string quantity, string absentIsNot, double airFlowRate_Lps, double value)
        {
            if (ventilationUnitAirFlowPerformance is null)
            {
                return string.Format(
                    "The ventilation unit template '{0}' states no {1}. Missing is not {2}, and no generic or system-template value is put in its place - it has to be transcribed from a certified source before it can be used.",
                    ventilationUnitReference,
                    quantity,
                    absentIsNot);
            }

            string invalidReason = ventilationUnitAirFlowPerformance.InvalidReason();
            if (invalidReason is not null)
            {
                return string.Format("The {0} of ventilation unit template '{1}' cannot be used: {2}.", quantity, ventilationUnitReference, invalidReason);
            }

            if (double.IsNaN(value))
            {
                double[] airFlowRates_Lps = ventilationUnitAirFlowPerformance.AirFlowRates_Lps;

                return string.Format(
                    "The {0} of ventilation unit template '{1}' is published for {2:0.###} to {3:0.###} l/s, and the design airflow {4:0.###} l/s lies outside that. The data states no behaviour beyond its published operating points, so nothing is extrapolated.",
                    quantity,
                    ventilationUnitReference,
                    airFlowRates_Lps[0],
                    airFlowRates_Lps[airFlowRates_Lps.Length - 1],
                    airFlowRate_Lps);
            }

            return null;
        }

        private static VentilationUnitOperatingParameters RefusedOperatingParameters(VentilationUnitReference ventilationUnitReference, double supplyAirFlowRate_Lps, double extractAirFlowRate_Lps, string reason)
        {
            return new VentilationUnitOperatingParameters(
                ventilationUnitReference,
                supplyAirFlowRate_Lps,
                extractAirFlowRate_Lps,
                double.NaN,
                double.NaN,
                HeatRecoveryEfficiencyBasis.Undefined,
                false,
                reason,
                double.NaN,
                SpecificFanPowerBasis.Undefined,
                false,
                reason);
        }

        private static bool IsPositiveFlowRate(double flowRate_Lps)
        {
            return !double.IsNaN(flowRate_Lps) && !double.IsInfinity(flowRate_Lps) && flowRate_Lps > 0;
        }
    }
}
