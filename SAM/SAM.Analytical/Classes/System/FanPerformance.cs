// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// A ventilation unit's certified specific fan power against airflow, in W/(l/s), on a stated basis.
    /// <para>
    /// Template data, carried on <see cref="VentilationUnitTemplate.FanPerformance"/> and read by
    /// <c>Query.VentilationUnitOperatingParameters</c> at the unit's design airflow. See
    /// <see cref="VentilationUnitAirFlowPerformance"/> for what is checked and why.
    /// </para>
    /// <para>
    /// <b>Indexed on airflow, and only on airflow.</b> Some manufacturers publish fan power against
    /// external static pressure instead. That is a different question - it needs a stated design pressure
    /// to be answered at all - and such a table is refused here rather than read at the wrong coordinate.
    /// </para>
    /// <para>
    /// <b>Not the brochure's electrical figures.</b> A catalogue <see cref="VentilationUnitTemplate.Source"/>
    /// that mentions fan input powers in words is not a specific fan power, and nothing reinterprets it as
    /// one.
    /// </para>
    /// </summary>
    public class FanPerformance : VentilationUnitAirFlowPerformance
    {
        private SpecificFanPowerBasis specificFanPowerBasis;

        public FanPerformance()
        {
        }

        /// <summary>
        /// Builds the data from paired airflows and specific fan powers.
        /// </summary>
        /// <param name="airFlowRates_Lps">The certified operating points [l/s], strictly increasing.</param>
        /// <param name="specificFanPowers_WPerLps">The matching specific fan powers [W/(l/s)], each positive.</param>
        /// <param name="specificFanPowerBasis">Which fans and which airflow each figure divides by.</param>
        /// <param name="source">The certified document the figures were transcribed from.</param>
        /// <param name="performanceDomainPolicy">Behaviour outside the published airflows. Refuse unless the source states otherwise.</param>
        public FanPerformance(IEnumerable<double> airFlowRates_Lps, IEnumerable<double> specificFanPowers_WPerLps, SpecificFanPowerBasis specificFanPowerBasis, string source, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
            : base(VentilationUnitPerformanceOutput.Name_SpecificFanPower, VentilationUnitPerformanceOutput.Unit_WattsPerLitrePerSecond, airFlowRates_Lps, specificFanPowers_WPerLps, source, performanceDomainPolicy)
        {
            this.specificFanPowerBasis = specificFanPowerBasis;
        }

        public FanPerformance(FanPerformance fanPerformance)
            : base(fanPerformance)
        {
            if (fanPerformance is not null)
            {
                specificFanPowerBasis = fanPerformance.specificFanPowerBasis;
            }
        }

        public FanPerformance(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>Which fans and which airflow the figures divide by. <see cref="SpecificFanPowerBasis.Undefined"/> makes the data unusable.</summary>
        public SpecificFanPowerBasis SpecificFanPowerBasis
        {
            get
            {
                return specificFanPowerBasis;
            }
        }

        /// <summary>
        /// The specific fan power [W/(l/s)] at an airflow [l/s], under the stored domain policy - or
        /// <see cref="double.NaN"/> where it cannot be stated. See <see cref="VentilationUnitAirFlowPerformance.Value"/>.
        /// </summary>
        public double SpecificFanPower_WPerLps(double airFlowRate_Lps)
        {
            return Value(airFlowRate_Lps);
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            //Absent or unrecognised both read as Undefined, which refuses the data - there is no default basis.
            string text = PerformanceJson.Text(jsonObject, "SpecificFanPowerBasis");
            specificFanPowerBasis = string.IsNullOrWhiteSpace(text) ? SpecificFanPowerBasis.Undefined : Core.Query.Enum<SpecificFanPowerBasis>(text);

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            if (specificFanPowerBasis != SpecificFanPowerBasis.Undefined)
            {
                result["SpecificFanPowerBasis"] = specificFanPowerBasis.ToString();
            }

            return result;
        }

        protected override string OutputName
        {
            get
            {
                return VentilationUnitPerformanceOutput.Name_SpecificFanPower;
            }
        }

        protected override string OutputUnit
        {
            get
            {
                return VentilationUnitPerformanceOutput.Unit_WattsPerLitrePerSecond;
            }
        }

        protected override bool IsValidValue(double value)
        {
            return value > 0 && !double.IsInfinity(value);
        }

        protected override string ValueRefusal(double value)
        {
            return string.Format(
                "it states a specific fan power of {0} W/(l/s), and a fan that moves air draws power - zero or less is a transcription mistake, refused rather than read as a free fan",
                value);
        }

        protected override string BasisRefusal()
        {
            return specificFanPowerBasis == SpecificFanPowerBasis.Undefined
                ? "it does not state which fans and which airflow its specific fan power divides by (SpecificFanPowerBasis), and a figure without a basis cannot be applied"
                : null;
        }
    }
}
