// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// A ventilation unit's certified sensible heat recovery efficiency against airflow, on a stated
    /// basis.
    /// <para>
    /// Template data, carried on <see cref="VentilationUnitTemplate.HeatRecoveryPerformance"/> and read by
    /// <c>Query.VentilationUnitOperatingParameters</c> at the unit's design airflow. See
    /// <see cref="VentilationUnitAirFlowPerformance"/> for what is checked and why.
    /// </para>
    /// <para>
    /// <b>What an absent one means.</b> Nothing. A template without heat recovery performance has not had
    /// its certified efficiency transcribed, and it is refused rather than given 0, or the efficiency some
    /// system template happens to ship with.
    /// </para>
    /// </summary>
    public class HeatRecoveryPerformance : VentilationUnitAirFlowPerformance
    {
        private HeatRecoveryEfficiencyBasis heatRecoveryEfficiencyBasis;

        public HeatRecoveryPerformance()
        {
        }

        /// <summary>
        /// Builds the data from paired airflows and efficiencies.
        /// </summary>
        /// <param name="airFlowRates_Lps">The certified operating points [l/s], strictly increasing.</param>
        /// <param name="sensibleHeatRecoveryEfficiencies">The matching efficiencies, each a fraction between 0 and 1.</param>
        /// <param name="heatRecoveryEfficiencyBasis">What each efficiency is a ratio of.</param>
        /// <param name="source">The certified document the figures were transcribed from.</param>
        /// <param name="performanceDomainPolicy">Behaviour outside the published airflows. Refuse unless the source states otherwise.</param>
        public HeatRecoveryPerformance(IEnumerable<double> airFlowRates_Lps, IEnumerable<double> sensibleHeatRecoveryEfficiencies, HeatRecoveryEfficiencyBasis heatRecoveryEfficiencyBasis, string source, PerformanceDomainPolicy performanceDomainPolicy = PerformanceDomainPolicy.Refuse)
            : base(VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency, VentilationUnitPerformanceOutput.Unit_Dimensionless, airFlowRates_Lps, sensibleHeatRecoveryEfficiencies, source, performanceDomainPolicy)
        {
            this.heatRecoveryEfficiencyBasis = heatRecoveryEfficiencyBasis;
        }

        public HeatRecoveryPerformance(HeatRecoveryPerformance heatRecoveryPerformance)
            : base(heatRecoveryPerformance)
        {
            if (heatRecoveryPerformance is not null)
            {
                heatRecoveryEfficiencyBasis = heatRecoveryPerformance.heatRecoveryEfficiencyBasis;
            }
        }

        public HeatRecoveryPerformance(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        /// <summary>What the efficiencies are a ratio of. <see cref="HeatRecoveryEfficiencyBasis.Undefined"/> makes the data unusable.</summary>
        public HeatRecoveryEfficiencyBasis HeatRecoveryEfficiencyBasis
        {
            get
            {
                return heatRecoveryEfficiencyBasis;
            }
        }

        /// <summary>
        /// The sensible heat recovery efficiency [-] at an airflow [l/s], under the stored domain policy -
        /// or <see cref="double.NaN"/> where it cannot be stated. See <see cref="VentilationUnitAirFlowPerformance.Value"/>.
        /// </summary>
        public double SensibleHeatRecoveryEfficiency(double airFlowRate_Lps)
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
            string text = PerformanceJson.Text(jsonObject, "HeatRecoveryEfficiencyBasis");
            heatRecoveryEfficiencyBasis = string.IsNullOrWhiteSpace(text) ? HeatRecoveryEfficiencyBasis.Undefined : Core.Query.Enum<HeatRecoveryEfficiencyBasis>(text);

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result is null)
            {
                return result;
            }

            if (heatRecoveryEfficiencyBasis != HeatRecoveryEfficiencyBasis.Undefined)
            {
                result["HeatRecoveryEfficiencyBasis"] = heatRecoveryEfficiencyBasis.ToString();
            }

            return result;
        }

        protected override string OutputName
        {
            get
            {
                return VentilationUnitPerformanceOutput.Name_SensibleHeatRecoveryEfficiency;
            }
        }

        protected override string OutputUnit
        {
            get
            {
                return VentilationUnitPerformanceOutput.Unit_Dimensionless;
            }
        }

        protected override bool IsValidValue(double value)
        {
            return value >= 0 && value <= 1;
        }

        protected override string ValueRefusal(double value)
        {
            return string.Format(
                "it states a sensible heat recovery efficiency of {0}, and an efficiency is a fraction between 0 and 1 - one outside that is a transcription mistake (a percentage typed as a whole number, for instance), refused rather than clamped",
                value);
        }

        protected override string BasisRefusal()
        {
            return heatRecoveryEfficiencyBasis == HeatRecoveryEfficiencyBasis.Undefined
                ? "it does not state what its efficiency is a ratio of (HeatRecoveryEfficiencyBasis), and an efficiency without a basis cannot be applied"
                : null;
        }
    }
}
