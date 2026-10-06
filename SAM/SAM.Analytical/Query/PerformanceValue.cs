// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The value of one published quantity at one set of named conditions.
        /// <para>
        /// <b>Conditions are named, not ordered.</b> A caller says
        /// <c>{ ExternalDryBulbTemperature = 30.5, EnteringDryBulbTemperature = 24, AirFlowRate = 85 }</c>
        /// and the axis order in the file is this method's problem. A positional call would make the order
        /// two axes were written in a load-bearing property of a hand-edited catalogue, and transposing
        /// two temperature axes is a mistake that produces plausible numbers.
        /// </para>
        /// <para>
        /// <b>Every axis has to be given a condition, and no extras.</b> A missing condition is refused
        /// rather than defaulted - there is no neutral value for "the outdoor temperature" - and an
        /// unrecognised name is refused rather than ignored, because a caller that misspells an axis is
        /// asking about something else and would otherwise be handed a confident answer to a question it
        /// did not ask.
        /// </para>
        /// </summary>
        /// <param name="ventilationUnitPerformanceTable">The published table to read.</param>
        /// <param name="outputName">
        /// Which published quantity - see the <c>Name_</c> constants on
        /// <see cref="VentilationUnitPerformanceOutput"/>.
        /// </param>
        /// <param name="conditions">
        /// One value per axis, keyed by the axis name - see the <c>Name_</c> constants on
        /// <see cref="VentilationUnitPerformanceAxis"/>.
        /// </param>
        /// <param name="performanceDomainPolicy">
        /// What to do beyond the published conditions. Defaults to
        /// <see cref="PerformanceDomainPolicy.Refuse"/> - see the enum.
        /// </param>
        /// <returns>The value, or <see cref="double.NaN"/> where the question cannot be answered.</returns>
        public static double PerformanceValue(this VentilationUnitPerformanceTable ventilationUnitPerformanceTable, string outputName, IDictionary<string, double> conditions, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
        {
            if (ventilationUnitPerformanceTable is null || !ventilationUnitPerformanceTable.IsValid || conditions is null)
            {
                return double.NaN;
            }

            List<string> axisNames = ventilationUnitPerformanceTable.AxisNames;

            if (axisNames is null || axisNames.Count != conditions.Count)
            {
                return double.NaN;
            }

            double[] coordinates = new double[axisNames.Count];

            for (int i = 0; i < axisNames.Count; i++)
            {
                if (!conditions.TryGetValue(axisNames[i], out double value))
                {
                    return double.NaN;
                }

                coordinates[i] = value;
            }

            return ventilationUnitPerformanceTable.Value(outputName, coordinates, performanceDomainPolicy);
        }

        /// <summary>
        /// The value of one published quantity from a template's performance table, at named conditions.
        /// See the table overload.
        /// </summary>
        public static double PerformanceValue(this VentilationUnitTemplate ventilationUnitTemplate, string outputName, IDictionary<string, double> conditions, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
        {
            return PerformanceValue(ventilationUnitTemplate?.PerformanceTable, outputName, conditions, performanceDomainPolicy);
        }

        /// <summary>
        /// The temperature [&#176;C] of the air the unit delivers, at an external dry bulb, an entering dry
        /// bulb and an airflow.
        /// <para>
        /// The convenience call for the three-condition arrangement a domestic heat recovery unit with
        /// cooling publishes on, named after what it means. It resolves its axes by name like every other
        /// lookup, so a template tabulated in a different axis order answers identically.
        /// </para>
        /// <para>
        /// <b>A lookup, not a control model.</b> It answers "what does the manufacturer say at these
        /// conditions". It does not decide what the airflow is, does not know what a timestep is, and
        /// produces no operating state. Iteration 3 owns those questions.
        /// </para>
        /// <para>
        /// <b>Named units, checked, not assumed.</b> This API's name promises &#176;C in, l/s in, &#176;C
        /// out. A table whose declared axis/output units do not match - a different unit system, or a
        /// manufacturer field left blank - is refused with <see cref="double.NaN"/> rather than having its
        /// raw numbers treated as if they were Celsius and litres per second. A table published in other
        /// units is still fully readable through the raw <see cref="PerformanceValue(VentilationUnitPerformanceTable, string, IDictionary{string, double}, PerformanceDomainPolicy)"/>.
        /// </para>
        /// </summary>
        public static double SupplyAirTemperature_C(this VentilationUnitTemplate ventilationUnitTemplate, double externalDryBulbTemperature_C, double enteringDryBulbTemperature_C, double airFlowRate_Lps, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
        {
            if (!HasTypedUnits(ventilationUnitTemplate?.PerformanceTable, VentilationUnitPerformanceOutput.Name_SupplyAirTemperature, VentilationUnitPerformanceOutput.Unit_DegreesCelsius))
            {
                return double.NaN;
            }

            return PerformanceValue(
                ventilationUnitTemplate,
                VentilationUnitPerformanceOutput.Name_SupplyAirTemperature,
                Conditions(externalDryBulbTemperature_C, enteringDryBulbTemperature_C, airFlowRate_Lps),
                performanceDomainPolicy);
        }

        /// <summary>
        /// The manufacturer's combined cooling figure [kW] at an external dry bulb, an entering dry bulb and
        /// an airflow. See <see cref="SupplyAirTemperature_C"/> - the same lookup, the same unit checking,
        /// the other output.
        /// </summary>
        public static double CombinedCoolingCapacity_kW(this VentilationUnitTemplate ventilationUnitTemplate, double externalDryBulbTemperature_C, double enteringDryBulbTemperature_C, double airFlowRate_Lps, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
        {
            if (!HasTypedUnits(ventilationUnitTemplate?.PerformanceTable, VentilationUnitPerformanceOutput.Name_CombinedCoolingCapacity, VentilationUnitPerformanceOutput.Unit_Kilowatts))
            {
                return double.NaN;
            }

            return PerformanceValue(
                ventilationUnitTemplate,
                VentilationUnitPerformanceOutput.Name_CombinedCoolingCapacity,
                Conditions(externalDryBulbTemperature_C, enteringDryBulbTemperature_C, airFlowRate_Lps),
                performanceDomainPolicy);
        }

        private static Dictionary<string, double> Conditions(double externalDryBulbTemperature_C, double enteringDryBulbTemperature_C, double airFlowRate_Lps)
        {
            return new Dictionary<string, double>
            {
                { VentilationUnitPerformanceAxis.Name_ExternalDryBulbTemperature, externalDryBulbTemperature_C },
                { VentilationUnitPerformanceAxis.Name_EnteringDryBulbTemperature, enteringDryBulbTemperature_C },
                { VentilationUnitPerformanceAxis.Name_AirFlowRate, airFlowRate_Lps },
            };
        }

        /// <summary>
        /// Whether a table declares exactly the units the &#176;C/l/s/kW typed lookups above promise: both
        /// temperature axes in Celsius, the airflow axis in litres per second, and the named output in its
        /// expected unit. A missing axis/output, or one whose <c>Unit</c> was left unset or names something
        /// else, refuses - there is no neutral unit to assume on a manufacturer's behalf.
        /// </summary>
        private static bool HasTypedUnits(VentilationUnitPerformanceTable ventilationUnitPerformanceTable, string outputName, string outputUnit)
        {
            if (ventilationUnitPerformanceTable is null)
            {
                return false;
            }

            if (!AxisUnitMatches(ventilationUnitPerformanceTable, VentilationUnitPerformanceAxis.Name_ExternalDryBulbTemperature, VentilationUnitPerformanceAxis.Unit_DegreesCelsius))
            {
                return false;
            }

            if (!AxisUnitMatches(ventilationUnitPerformanceTable, VentilationUnitPerformanceAxis.Name_EnteringDryBulbTemperature, VentilationUnitPerformanceAxis.Unit_DegreesCelsius))
            {
                return false;
            }

            if (!AxisUnitMatches(ventilationUnitPerformanceTable, VentilationUnitPerformanceAxis.Name_AirFlowRate, VentilationUnitPerformanceAxis.Unit_LitresPerSecond))
            {
                return false;
            }

            VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput = ventilationUnitPerformanceTable.Output(outputName);

            return ventilationUnitPerformanceOutput is not null && string.Equals(ventilationUnitPerformanceOutput.Unit, outputUnit, StringComparison.Ordinal);
        }

        private static bool AxisUnitMatches(VentilationUnitPerformanceTable ventilationUnitPerformanceTable, string axisName, string unit)
        {
            VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis = ventilationUnitPerformanceTable.Axis(axisName);

            return ventilationUnitPerformanceAxis is not null && string.Equals(ventilationUnitPerformanceAxis.Unit, unit, StringComparison.Ordinal);
        }
    }
}
