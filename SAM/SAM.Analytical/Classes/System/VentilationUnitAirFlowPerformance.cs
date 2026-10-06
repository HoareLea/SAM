// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// One quantity a manufacturer certified against airflow - a heat recovery efficiency, a specific fan
    /// power - as the published operating points, the document they came from, and what may be said
    /// beyond them.
    /// <para>
    /// <b>A typed view of a <see cref="VentilationUnitPerformanceTable"/>, not a second table format.</b>
    /// The points are held in the existing table grammar - one <see cref="VentilationUnitPerformanceAxis.Name_AirFlowRate"/>
    /// axis in l/s, one output - so they are stored raw, read back exactly at a published point, and
    /// interpolated only at use. What this adds is the one thing a raw table cannot say: that the numbers
    /// mean what the typed accessor promises. <see cref="IsValid"/> proves the axis, the units, the value
    /// range, the stated basis and the source, the same way <see cref="FlowFractionControlCurve"/> proves
    /// its Celsius axis rather than trusting a name.
    /// </para>
    /// <para>
    /// <b>Absent is not zero.</b> A template that carries no such object states nothing about the quantity,
    /// and <c>Query.VentilationUnitOperatingParameters</c> refuses to resolve it - it never reads a missing
    /// efficiency as 0, a missing fan power as 0, or borrows a figure from anywhere else.
    /// </para>
    /// <para>
    /// <b>Its domain policy is stored and defaults to refusing.</b> Only
    /// <see cref="PerformanceDomainPolicy.Refuse"/> and <see cref="PerformanceDomainPolicy.ClampToDomain"/>
    /// may be stated. <see cref="PerformanceDomainPolicy.OuterCellLinearExtrapolation"/> makes the data
    /// unusable: a certified figure at an airflow the manufacturer never tested is not certified.
    /// </para>
    /// </summary>
    public abstract class VentilationUnitAirFlowPerformance : IJSAMObject
    {
        private VentilationUnitPerformanceTable ventilationUnitPerformanceTable;
        private PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse;
        private string source;

        protected VentilationUnitAirFlowPerformance()
        {
        }

        protected VentilationUnitAirFlowPerformance(string outputName, string outputUnit, IEnumerable<double> airFlowRates_Lps, IEnumerable<double> values, string source, PerformanceDomainPolicy performanceDomainPolicy)
        {
            ventilationUnitPerformanceTable = new VentilationUnitPerformanceTable(
                [new VentilationUnitPerformanceAxis(VentilationUnitPerformanceAxis.Name_AirFlowRate, VentilationUnitPerformanceAxis.Unit_LitresPerSecond, airFlowRates_Lps)],
                [new VentilationUnitPerformanceOutput(outputName, outputUnit, values)]);

            this.source = source;
            this.performanceDomainPolicy = performanceDomainPolicy;
        }

        protected VentilationUnitAirFlowPerformance(VentilationUnitAirFlowPerformance ventilationUnitAirFlowPerformance)
        {
            if (ventilationUnitAirFlowPerformance is not null)
            {
                ventilationUnitPerformanceTable = ventilationUnitAirFlowPerformance.ventilationUnitPerformanceTable is null ? null : new VentilationUnitPerformanceTable(ventilationUnitAirFlowPerformance.ventilationUnitPerformanceTable);
                performanceDomainPolicy = ventilationUnitAirFlowPerformance.performanceDomainPolicy;
                source = ventilationUnitAirFlowPerformance.source;
            }
        }

        protected VentilationUnitAirFlowPerformance(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>What this data does at an airflow outside the published operating points.</summary>
        public PerformanceDomainPolicy PerformanceDomainPolicy
        {
            get
            {
                return performanceDomainPolicy;
            }
        }

        /// <summary>
        /// The certified document these figures were transcribed from - its title, issue, and the entry
        /// they appear in.
        /// <para>
        /// <b>Required.</b> Held here as well as on <see cref="VentilationUnitTemplate.Source"/> because
        /// certified performance and a brochure are routinely different documents, and a figure that cannot
        /// be traced to the one that certifies it is a number in a file, not manufacturer data.
        /// </para>
        /// </summary>
        public string Source
        {
            get
            {
                return source;
            }
        }

        /// <summary>The published airflows [l/s], in order. Null unless <see cref="IsValid"/>.</summary>
        public double[] AirFlowRates_Lps
        {
            get
            {
                VentilationUnitPerformanceTable ventilationUnitPerformanceTable = this.ventilationUnitPerformanceTable;

                return InvalidReason(ventilationUnitPerformanceTable) is null ? ventilationUnitPerformanceTable.Axis(0).Values : null;
            }
        }

        /// <summary>The published values, one per airflow, in the unit the typed accessor names. Null unless <see cref="IsValid"/>.</summary>
        public double[] Values
        {
            get
            {
                VentilationUnitPerformanceTable ventilationUnitPerformanceTable = this.ventilationUnitPerformanceTable;

                return InvalidReason(ventilationUnitPerformanceTable) is null ? ventilationUnitPerformanceTable.Output(OutputName).Values : null;
            }
        }

        /// <summary>
        /// Whether the data can be read at all: one airflow axis in l/s, every airflow positive, exactly the
        /// one expected output in its expected unit, every value in range, a stated basis, a domain policy
        /// that does not extrapolate, and a source.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return InvalidReason() is null;
            }
        }

        /// <summary>Whether an airflow [l/s] falls within the published operating points. A published boundary is inside.</summary>
        public bool InDomain(double airFlowRate_Lps)
        {
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = this.ventilationUnitPerformanceTable;

            return InvalidReason(ventilationUnitPerformanceTable) is null && ventilationUnitPerformanceTable.InDomain(airFlowRate_Lps);
        }

        public override string ToString()
        {
            double[] airFlowRates_Lps = AirFlowRates_Lps;
            double[] values = Values;

            if (airFlowRates_Lps is null || values is null)
            {
                return string.Format("Invalid {0}", GetType().Name);
            }

            List<string> points = [];
            for (int i = 0; i < airFlowRates_Lps.Length; i++)
            {
                points.Add(string.Format("{0:0.###} l/s -> {1:0.###}", airFlowRates_Lps[i], values[i]));
            }

            return string.Format("{0} ({1} outside)", string.Join(", ", points), performanceDomainPolicy);
        }

        public virtual bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject is null)
            {
                return false;
            }

            ventilationUnitPerformanceTable = jsonObject["PerformanceTable"] is JsonObject jsonObject_PerformanceTable ? new VentilationUnitPerformanceTable(jsonObject_PerformanceTable) : null;
            source = PerformanceJson.Text(jsonObject, "Source");

            //ABSENT reads as Refuse - the strict answer, so a file that says nothing gets nothing beyond its
            //published points. PRESENT but not a recognised name reads as Undefined, which IsValid refuses: a
            //mistyped policy must never quietly become a clamp.
            performanceDomainPolicy = PerformanceDomainPolicy.Refuse;

            if (jsonObject.ContainsKey("PerformanceDomainPolicy"))
            {
                string text = PerformanceJson.Text(jsonObject, "PerformanceDomainPolicy");

                performanceDomainPolicy = string.IsNullOrWhiteSpace(text) ? PerformanceDomainPolicy.Undefined : Core.Query.Enum<PerformanceDomainPolicy>(text);
            }

            return true;
        }

        public virtual JsonObject ToJsonObject()
        {
            JsonObject result = new()
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            if (ventilationUnitPerformanceTable is not null)
            {
                result["PerformanceTable"] = ventilationUnitPerformanceTable.ToJsonObject();
            }

            result["PerformanceDomainPolicy"] = performanceDomainPolicy.ToString();

            PerformanceJson.SetText(result, "Source", source);

            return result;
        }

        /// <summary>The output name this data publishes - see the <c>Name_</c> constants on <see cref="VentilationUnitPerformanceOutput"/>.</summary>
        protected abstract string OutputName { get; }

        /// <summary>The unit that output has to declare.</summary>
        protected abstract string OutputUnit { get; }

        /// <summary>Whether one published value is physically possible for this quantity.</summary>
        protected abstract bool IsValidValue(double value);

        /// <summary>The sentence refusing a value <see cref="IsValidValue"/> rejected.</summary>
        protected abstract string ValueRefusal(double value);

        /// <summary>The sentence refusing data whose basis is not stated, or null where it is.</summary>
        protected abstract string BasisRefusal();

        /// <summary>
        /// The published value at an airflow [l/s], under the stored domain policy. <see cref="double.NaN"/>
        /// where the data is not valid, the airflow is not a finite number, or it lies outside the published
        /// operating points and the policy is to refuse.
        /// <para>
        /// Exactly the published number at a published airflow; linear between two of them.
        /// </para>
        /// </summary>
        protected double Value(double airFlowRate_Lps)
        {
            //ONE capture - the validity this proves has to be the validity of the table that is then read.
            VentilationUnitPerformanceTable ventilationUnitPerformanceTable = this.ventilationUnitPerformanceTable;

            if (InvalidReason(ventilationUnitPerformanceTable) is not null || double.IsNaN(airFlowRate_Lps) || double.IsInfinity(airFlowRate_Lps))
            {
                return double.NaN;
            }

            return ventilationUnitPerformanceTable.Value(OutputName, [airFlowRate_Lps], performanceDomainPolicy);
        }

        /// <summary>Why this data cannot be read, as a clause - or null where it can.</summary>
        internal string InvalidReason()
        {
            return InvalidReason(ventilationUnitPerformanceTable);
        }

        private string InvalidReason(VentilationUnitPerformanceTable ventilationUnitPerformanceTable)
        {
            if (ventilationUnitPerformanceTable is null || !ventilationUnitPerformanceTable.IsValid)
            {
                return "its published table is missing or malformed - every airflow and every value has to be a finite number, the airflows strictly increasing so that no operating point is stated twice, and exactly one value given per airflow";
            }

            if (ventilationUnitPerformanceTable.AxisCount != 1 || ventilationUnitPerformanceTable.AxisIndex(VentilationUnitPerformanceAxis.Name_AirFlowRate) != 0)
            {
                return string.Format(
                    "it is not tabulated against airflow alone (axes: {0}), and a table indexed on anything else - external static pressure, for instance - cannot be read at a design airflow",
                    string.Join(", ", ventilationUnitPerformanceTable.AxisNames));
            }

            VentilationUnitPerformanceAxis ventilationUnitPerformanceAxis = ventilationUnitPerformanceTable.Axis(0);

            if (!string.Equals(ventilationUnitPerformanceAxis.Unit, VentilationUnitPerformanceAxis.Unit_LitresPerSecond, StringComparison.Ordinal))
            {
                return string.Format("its airflow axis is declared in '{0}' rather than '{1}'", ventilationUnitPerformanceAxis.Unit ?? string.Empty, VentilationUnitPerformanceAxis.Unit_LitresPerSecond);
            }

            if (ventilationUnitPerformanceAxis.Minimum <= 0)
            {
                return string.Format("it states an operating point at {0:0.###} l/s, and a unit moving no air, or air backwards, has no performance to state", ventilationUnitPerformanceAxis.Minimum);
            }

            List<string> outputNames = ventilationUnitPerformanceTable.OutputNames;

            if (outputNames.Count != 1 || !string.Equals(outputNames[0], OutputName, StringComparison.Ordinal))
            {
                return string.Format("it does not publish exactly one quantity, {0} (outputs: {1})", OutputName, string.Join(", ", outputNames));
            }

            VentilationUnitPerformanceOutput ventilationUnitPerformanceOutput = ventilationUnitPerformanceTable.Output(OutputName);

            if (!string.Equals(ventilationUnitPerformanceOutput.Unit, OutputUnit, StringComparison.Ordinal))
            {
                return string.Format("its {0} is declared in '{1}' rather than '{2}', and it is not converted on anybody's behalf", OutputName, ventilationUnitPerformanceOutput.Unit ?? string.Empty, OutputUnit);
            }

            foreach (double value in ventilationUnitPerformanceOutput.Values)
            {
                if (!IsValidValue(value))
                {
                    return ValueRefusal(value);
                }
            }

            string basisRefusal = BasisRefusal();
            if (basisRefusal is not null)
            {
                return basisRefusal;
            }

            if (performanceDomainPolicy != PerformanceDomainPolicy.Refuse && performanceDomainPolicy != PerformanceDomainPolicy.ClampToDomain)
            {
                return string.Format(
                    "its domain policy is '{0}' - only Refuse or ClampToDomain may be stated, because a certified figure at an airflow the manufacturer never tested is not certified, and nothing is extrapolated",
                    performanceDomainPolicy);
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                return "it states no source, so its figures cannot be traced to the document that certifies them";
            }

            return null;
        }
    }
}
