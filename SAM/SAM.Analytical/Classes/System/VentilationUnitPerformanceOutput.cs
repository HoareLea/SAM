// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One quantity a manufacturer publishes over a performance table's axes - its name, its unit, and
    /// every published value, flattened.
    /// <para>
    /// <b>One output per quantity, and a table may carry any number of them.</b> The Nuaire hybrid
    /// cooling table publishes two - a supply air temperature and a combined cooling capacity - over the
    /// same three conditions. Another manufacturer will publish a different set, and adding one is adding
    /// an entry to a list rather than changing a schema. That is what keeps the catalogue format from
    /// having to be reopened the first time a product publishes an electrical input power.
    /// </para>
    /// <para>
    /// <b>Flattened row-major, last axis fastest.</b> The same layout
    /// <c>SAM.Math.MultilinearInterpolation</c> reads, so no reshaping happens between the file and the
    /// lookup: for axes of lengths (3, 4, 8) the value at (i, j, k) sits at index ((i * 4) + j) * 8 + k.
    /// The count has to match the product of the axis lengths exactly, which is the check that catches a
    /// transcription that dropped or doubled a row.
    /// </para>
    /// </summary>
    public class VentilationUnitPerformanceOutput : IJSAMObject
    {
        /// <summary>
        /// The temperature of the air the unit delivers to the space - the manufacturer's "supply air", and
        /// the quantity a later iteration writes into an air handling unit zone's thermostat profile.
        /// </summary>
        public const string Name_SupplyAirTemperature = "SupplyAirTemperature";

        /// <summary>
        /// The manufacturer's combined figure for what the unit removes - for a hybrid heat recovery and
        /// direct expansion product, coolth recovery and sensible cooling together.
        /// </summary>
        public const string Name_CombinedCoolingCapacity = "CombinedCoolingCapacity";

        /// <summary>The fraction of the unit's airflow a controller calls for. Dimensionless, 0 to 1.</summary>
        public const string Name_FlowFraction = "FlowFraction";

        /// <summary>
        /// The unit's sensible heat recovery efficiency. Dimensionless, 0 to 1, on the basis
        /// <see cref="HeatRecoveryPerformance"/> states beside it.
        /// </summary>
        public const string Name_SensibleHeatRecoveryEfficiency = "SensibleHeatRecoveryEfficiency";

        /// <summary>
        /// The unit's specific fan power - electrical input per unit of airflow - in
        /// <see cref="Unit_WattsPerLitrePerSecond"/>, on the basis <see cref="FanPerformance"/> states
        /// beside it.
        /// </summary>
        public const string Name_SpecificFanPower = "SpecificFanPower";

        /// <summary>The unit string a Celsius-denominated output is expected to declare.</summary>
        public const string Unit_DegreesCelsius = "degC";

        /// <summary>The unit string a kilowatt-denominated output is expected to declare.</summary>
        public const string Unit_Kilowatts = "kW";

        /// <summary>The unit string a specific fan power output is expected to declare - watts per litre per second.</summary>
        public const string Unit_WattsPerLitrePerSecond = "W/(l/s)";

        /// <summary>The unit string a dimensionless output - a fraction with no unit of its own - is expected to declare.</summary>
        public const string Unit_Dimensionless = "-";

        private string name;
        private string unit;
        private double[] values;

        public VentilationUnitPerformanceOutput()
        {
        }

        public VentilationUnitPerformanceOutput(string name, string unit, IEnumerable<double> values)
        {
            this.name = name;
            this.unit = unit;
            this.values = values is null ? null : new List<double>(values).ToArray();
        }

        public VentilationUnitPerformanceOutput(VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput)
        {
            if (ventilationUnitPerformanceOutput is not null)
            {
                name = ventilationUnitPerformanceOutput.name;
                unit = ventilationUnitPerformanceOutput.unit;
                values = ventilationUnitPerformanceOutput.values is null ? null : (double[])ventilationUnitPerformanceOutput.values.Clone();
            }
        }

        public VentilationUnitPerformanceOutput(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>What quantity this is. See the <c>Name_</c> constants for the well-known ones.</summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>The unit the values are published in, verbatim from the source - "degC", "kW".</summary>
        public string Unit
        {
            get
            {
                return unit;
            }
        }

        /// <summary>Every published value, flattened. A copy.</summary>
        public double[] Values
        {
            get
            {
                return values is null ? null : (double[])values.Clone();
            }
        }

        /// <summary>How many values were published. -1 where none were.</summary>
        public int Count
        {
            get
            {
                return values is null ? -1 : values.Length;
            }
        }

        /// <summary>
        /// Whether this is an output a lookup can be answered from: it is named, and every published value
        /// is a finite number.
        /// <para>
        /// A gap is refused rather than carried, for the reason <c>SAM.Math.MultilinearInterpolation</c>
        /// refuses one: a missing corner makes every query touching its cell answer <see cref="double.NaN"/>,
        /// which is indistinguishable from "outside the published range" and would turn a transcription
        /// mistake into what looks like a deliberate domain limit.
        /// </para>
        /// <para>
        /// Whether the count lines up with the table's axes is the <i>table's</i> question, not this
        /// object's - see <c>VentilationUnitPerformanceTable.IsValid</c>.
        /// </para>
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name) || values is null || values.Length == 0)
                {
                    return false;
                }

                foreach (double value in values)
                {
                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>Whether this output is the named one, ordinally.</summary>
        public bool Matches(string name)
        {
            return string.Equals(this.name, name, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return string.Format(
                "{0} [{1}] {2} value(s)",
                string.IsNullOrWhiteSpace(name) ? "-" : name,
                string.IsNullOrWhiteSpace(unit) ? "-" : unit,
                values is null ? 0 : values.Length);
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            name = PerformanceJson.Text(jsonObject, "Name");
            unit = PerformanceJson.Text(jsonObject, "Unit");
            values = PerformanceJson.Values(jsonObject, "Values");

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject result = new()
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            PerformanceJson.SetText(result, "Name", name);
            PerformanceJson.SetText(result, "Unit", unit);
            PerformanceJson.SetValues(result, "Values", values);

            return result;
        }
    }
}
