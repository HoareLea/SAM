// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One independent condition a manufacturer tabulated performance against - its name, its unit, and
    /// the exact coordinates the manufacturer published, in order.
    /// <para>
    /// <b>The published coordinates, not a range.</b> A brochure states the conditions it measured at, and
    /// those conditions are the data: <c>[29, 32, 34]</c> external dry bulb is three measurements, not a
    /// span from 29 to 34 that happens to have been sampled. Storing the coordinates keeps the difference,
    /// so a lookup can return a published number exactly at a published condition and an interpolation
    /// policy can say what happens between and beyond them.
    /// </para>
    /// <para>
    /// <b>The unit is carried, never converted.</b> Manufacturer data is preserved in the units it was
    /// published in - degrees Celsius, litres per second - because a value that has been through a
    /// conversion is no longer the number in the document, and this library's whole reason for holding raw
    /// data is that somebody can check it against the document. Conversion, where a consumer needs it, is
    /// that consumer's business.
    /// </para>
    /// </summary>
    public class VentilationUnitPerformanceAxis : IJSAMObject
    {
        /// <summary>External/outdoor dry bulb temperature - the condition outside the building.</summary>
        public const string Name_ExternalDryBulbTemperature = "ExternalDryBulbTemperature";

        /// <summary>
        /// Entering dry bulb temperature - the air arriving at the unit from the space it serves, which is
        /// the room temperature in a single-dwelling heat recovery arrangement.
        /// </summary>
        public const string Name_EnteringDryBulbTemperature = "EnteringDryBulbTemperature";

        /// <summary>
        /// The airflow the performance was measured at.
        /// <para>
        /// <b>A duty point on a performance table, and nothing else.</b> The airflows an axis lists are the
        /// conditions the manufacturer chose to publish at. They are not unit sizes, and the largest of them
        /// is not the unit's maximum airflow - see
        /// <see cref="VentilationUnitTemplate.MaximumSupplyFlowRate_Lps"/>, which is a separate, separately
        /// sourced fact.
        /// </para>
        /// </summary>
        public const string Name_AirFlowRate = "AirFlowRate";

        /// <summary>The control temperature a flow-fraction curve is indexed by.</summary>
        public const string Name_ControlTemperature = "ControlTemperature";

        /// <summary>The unit string a Celsius-denominated axis is expected to declare.</summary>
        public const string Unit_DegreesCelsius = "degC";

        /// <summary>The unit string a litres-per-second-denominated axis is expected to declare.</summary>
        public const string Unit_LitresPerSecond = "l/s";

        private string name;
        private string unit;
        private double[] values;

        public VentilationUnitPerformanceAxis()
        {
        }

        public VentilationUnitPerformanceAxis(string name, string unit, IEnumerable<double> values)
        {
            this.name = name;
            this.unit = unit;
            this.values = values is null ? null : new List<double>(values).ToArray();
        }

        public VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis)
        {
            if (ventilationUnitPerformanceAxis is not null)
            {
                name = ventilationUnitPerformanceAxis.name;
                unit = ventilationUnitPerformanceAxis.unit;
                values = ventilationUnitPerformanceAxis.values is null ? null : (double[])ventilationUnitPerformanceAxis.values.Clone();
            }
        }

        public VentilationUnitPerformanceAxis(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>What condition this axis is. See the <c>Name_</c> constants for the well-known ones.</summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>The unit the coordinates are published in, verbatim from the source - "degC", "l/s".</summary>
        public string Unit
        {
            get
            {
                return unit;
            }
        }

        /// <summary>The published coordinates, in order. A copy.</summary>
        public double[] Values
        {
            get
            {
                return values is null ? null : (double[])values.Clone();
            }
        }

        /// <summary>How many conditions the manufacturer published on this axis.</summary>
        public int Count
        {
            get
            {
                return values is null ? -1 : values.Length;
            }
        }

        /// <summary>The lowest published coordinate.</summary>
        public double Minimum
        {
            get
            {
                return values is null || values.Length == 0 ? double.NaN : values[0];
            }
        }

        /// <summary>The highest published coordinate.</summary>
        public double Maximum
        {
            get
            {
                return values is null || values.Length == 0 ? double.NaN : values[values.Length - 1];
            }
        }

        /// <summary>
        /// Whether this is an axis a lookup can be indexed by: it is named, and it lists at least one finite
        /// coordinate, strictly increasing.
        /// <para>
        /// Strictly increasing rather than merely present, for the reason
        /// <c>SAM.Math.MultilinearInterpolation</c> refuses a repeated coordinate: two rows tabulated at the
        /// same condition give a zero-width cell, and nothing can honestly choose between the values on it.
        /// A hand-transcribed table with a duplicated or transposed row is a transcription error, and it is
        /// refused loudly rather than interpolated over.
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

                for (int i = 0; i < values.Length; i++)
                {
                    if (double.IsNaN(values[i]) || double.IsInfinity(values[i]))
                    {
                        return false;
                    }

                    if (i > 0 && values[i] <= values[i - 1])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>Whether this axis is the named one, ordinally. Names come from a file and are not a dialect.</summary>
        public bool Matches(string name)
        {
            return string.Equals(this.name, name, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return string.Format(
                "{0} [{1}] {2}",
                string.IsNullOrWhiteSpace(name) ? "-" : name,
                string.IsNullOrWhiteSpace(unit) ? "-" : unit,
                values is null ? "-" : string.Join(", ", System.Array.ConvertAll(values, x => x.ToString("0.###"))));
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
