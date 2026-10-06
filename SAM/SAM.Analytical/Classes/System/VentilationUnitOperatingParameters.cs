// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    /// <summary>
    /// What a selected ventilation unit product's own certified data says it does at one design duty: its
    /// sensible heat recovery efficiency and its specific fan power - each either resolved, or refused
    /// with a sentence saying why.
    /// <para>
    /// <b>Produced by <c>Query.VentilationUnitOperatingParameters</c> and by nothing else.</b> Immutable,
    /// and a pure function of the template and the duty it was asked about, so a caller may memoise it
    /// per (product, duty) pair.
    /// </para>
    /// <para>
    /// <b>The two quantities are resolved independently.</b> A unit whose fan power is certified and whose
    /// heat recovery efficiency is not yet transcribed resolves one and refuses the other, and a caller
    /// needing only the first can proceed. A caller needing both checks both - a refused quantity is never
    /// a zero, and <see cref="double.NaN"/> is its only value.
    /// </para>
    /// <para>
    /// <b>The airflows on this object are the design duty it was asked about, echoed - never an output.</b>
    /// Nothing here is a design airflow, a capacity or an operating airflow, and nothing here is ever
    /// written back to a model. The four quantities stay apart:
    /// </para>
    /// <code>
    /// PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity != OperatingAirFlow
    /// </code>
    /// </summary>
    public class VentilationUnitOperatingParameters
    {
        private readonly VentilationUnitReference ventilationUnitReference;

        internal VentilationUnitOperatingParameters(
            VentilationUnitReference ventilationUnitReference,
            double supplyAirFlowRate_Lps,
            double extractAirFlowRate_Lps,
            double airFlowRate_Lps,
            double sensibleHeatRecoveryEfficiency,
            HeatRecoveryEfficiencyBasis heatRecoveryEfficiencyBasis,
            bool heatRecoveryClampedToDomain,
            string heatRecoveryRefusal,
            double specificFanPower_WPerLps,
            SpecificFanPowerBasis specificFanPowerBasis,
            bool fanPerformanceClampedToDomain,
            string fanPerformanceRefusal)
        {
            //Copied: the caller's reference is mutable, and this object is meant to be safe to keep.
            this.ventilationUnitReference = ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitReference);

            SupplyAirFlowRate_Lps = supplyAirFlowRate_Lps;
            ExtractAirFlowRate_Lps = extractAirFlowRate_Lps;
            AirFlowRate_Lps = airFlowRate_Lps;

            SensibleHeatRecoveryEfficiency = sensibleHeatRecoveryEfficiency;
            HeatRecoveryEfficiencyBasis = heatRecoveryEfficiencyBasis;
            HeatRecoveryClampedToDomain = heatRecoveryClampedToDomain;
            HeatRecoveryRefusal = heatRecoveryRefusal;

            SpecificFanPower_WPerLps = specificFanPower_WPerLps;
            SpecificFanPowerBasis = specificFanPowerBasis;
            FanPerformanceClampedToDomain = fanPerformanceClampedToDomain;
            FanPerformanceRefusal = fanPerformanceRefusal;
        }

        /// <summary>The product the data was resolved for, or null where none was named. A copy.</summary>
        public VentilationUnitReference VentilationUnitReference
        {
            get
            {
                return ventilationUnitReference is null ? null : new VentilationUnitReference(ventilationUnitReference);
            }
        }

        /// <summary>The supply design airflow [l/s] the caller asked about, exactly as given.</summary>
        public double SupplyAirFlowRate_Lps { get; }

        /// <summary>The extract design airflow [l/s] the caller asked about, exactly as given.</summary>
        public double ExtractAirFlowRate_Lps { get; }

        /// <summary>
        /// The balanced airflow [l/s] both tables were read at - the mean of the two design airflows, which
        /// agree to within the tolerance the caller named. <see cref="double.NaN"/> where the duty could not
        /// be read at all.
        /// </summary>
        public double AirFlowRate_Lps { get; }

        /// <summary>The certified sensible heat recovery efficiency [-] at <see cref="AirFlowRate_Lps"/>, or <see cref="double.NaN"/> where refused.</summary>
        public double SensibleHeatRecoveryEfficiency { get; }

        /// <summary>What <see cref="SensibleHeatRecoveryEfficiency"/> is a ratio of. Undefined where refused.</summary>
        public HeatRecoveryEfficiencyBasis HeatRecoveryEfficiencyBasis { get; }

        /// <summary>
        /// Whether the efficiency was held at the nearest published operating point because the design
        /// airflow lies beyond them and the data states <see cref="PerformanceDomainPolicy.ClampToDomain"/>.
        /// A report has to say so: the figure is then the manufacturer's statement about its edge, not a
        /// measurement at this airflow.
        /// </summary>
        public bool HeatRecoveryClampedToDomain { get; }

        /// <summary>Why the efficiency could not be resolved, in words. Null where it was.</summary>
        public string HeatRecoveryRefusal { get; }

        /// <summary>Whether <see cref="SensibleHeatRecoveryEfficiency"/> is a resolved, certified figure.</summary>
        public bool IsHeatRecoveryResolved
        {
            get
            {
                return HeatRecoveryRefusal is null;
            }
        }

        /// <summary>The certified specific fan power [W/(l/s)] at <see cref="AirFlowRate_Lps"/>, or <see cref="double.NaN"/> where refused.</summary>
        public double SpecificFanPower_WPerLps { get; }

        /// <summary>Which fans and airflow <see cref="SpecificFanPower_WPerLps"/> divides by. Undefined where refused.</summary>
        public SpecificFanPowerBasis SpecificFanPowerBasis { get; }

        /// <summary>Whether the fan power was held at the nearest published operating point. See <see cref="HeatRecoveryClampedToDomain"/>.</summary>
        public bool FanPerformanceClampedToDomain { get; }

        /// <summary>Why the fan power could not be resolved, in words. Null where it was.</summary>
        public string FanPerformanceRefusal { get; }

        /// <summary>Whether <see cref="SpecificFanPower_WPerLps"/> is a resolved, certified figure.</summary>
        public bool IsFanPerformanceResolved
        {
            get
            {
                return FanPerformanceRefusal is null;
            }
        }

        public override string ToString()
        {
            string heatRecovery = IsHeatRecoveryResolved
                ? string.Format("sensible heat recovery efficiency {0:0.####} ({1}{2})", SensibleHeatRecoveryEfficiency, HeatRecoveryEfficiencyBasis, HeatRecoveryClampedToDomain ? ", clamped to the published range" : string.Empty)
                : "heat recovery refused";

            string fanPerformance = IsFanPerformanceResolved
                ? string.Format("specific fan power {0:0.####} W/(l/s) ({1}{2})", SpecificFanPower_WPerLps, SpecificFanPowerBasis, FanPerformanceClampedToDomain ? ", clamped to the published range" : string.Empty)
                : "fan performance refused";

            return string.Format(
                "{0} at supply {1:0.###} l/s, extract {2:0.###} l/s: {3}; {4}",
                ventilationUnitReference is null ? "-" : ventilationUnitReference.ToString(),
                SupplyAirFlowRate_Lps,
                ExtractAirFlowRate_Lps,
                heatRecovery,
                fanPerformance);
        }
    }
}
