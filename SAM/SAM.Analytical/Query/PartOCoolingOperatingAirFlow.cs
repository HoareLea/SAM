// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Globalization;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The airflow a cooled dwelling's unit moves while its cooling-stat calls - an <b>operating</b> airflow,
        /// never a design airflow and never the unit's capacity:
        /// <code>
        /// cooling operating airflow = max(design total, guidance airflow)
        ///   design total     = max(design supply, design extract) - the dwelling's terminals, read only
        ///   guidance airflow = the strategy's resolved figure, else the manufacturer's stated default
        /// valid only within the manufacturer's published cooling airflow range and the unit's capacity
        /// </code>
        /// <para>
        /// "Elevated" is never below what the dwelling is designed to move: ventilation does not drop below the
        /// accepted design while cooling. The guidance default is an operating point, not a limit; the published
        /// range is where the manufacturer's cooling data exists, so beyond it the cooling is refused rather than
        /// extrapolated (<c>documentation/PartO-MixedDwellingStrategies-PR3A.md</c> §14.2). Nothing here is stored as a
        /// design value.
        /// </para>
        /// </summary>
        /// <param name="ventilationUnitTemplate">The dwelling's selected product, with its manufacturer operating strategy.</param>
        /// <param name="designSupply_Lps">The dwelling's design supply total [l/s].</param>
        /// <param name="designExtract_Lps">The dwelling's design extract total [l/s].</param>
        /// <param name="refusal">Why no cooling operating airflow exists, or null.</param>
        /// <returns>The cooling operating airflow [l/s], or NaN where <paramref name="refusal"/> is set.</returns>
        public static double PartOCoolingOperatingAirFlow(this VentilationUnitTemplate ventilationUnitTemplate, double designSupply_Lps, double designExtract_Lps, out string refusal)
        {
            refusal = null;

            VentilationUnitOperatingStrategy strategy = ventilationUnitTemplate?.OperatingStrategy;
            if (strategy is null)
            {
                refusal = "publishes no manufacturer operating strategy, so it has no cooling guidance.";
                return double.NaN;
            }

            string refusal_Template = strategy.TemplateRefusal();
            if (refusal_Template is not null)
            {
                refusal = string.Format("has an operating strategy that {0}", refusal_Template);
                return double.NaN;
            }

            double guidance_Lps = double.IsNaN(strategy.ElevatedAirFlow_Lps) ? strategy.DefaultElevatedAirFlow_Lps : strategy.ElevatedAirFlow_Lps;
            if (!IsFinite(guidance_Lps) || guidance_Lps <= 0)
            {
                refusal = "states no cooling airflow - neither a resolved figure nor a default.";
                return double.NaN;
            }

            double minimum_Lps = strategy.MinimumElevatedAirFlow_Lps;
            double maximum_Lps = strategy.MaximumElevatedAirFlow_Lps;
            if (!IsFinite(minimum_Lps) || !IsFinite(maximum_Lps) || minimum_Lps > maximum_Lps)
            {
                refusal = "publishes no cooling airflow range, so where its cooling data exists is not stated.";
                return double.NaN;
            }

            if (!IsFinite(designSupply_Lps) || !IsFinite(designExtract_Lps) || designSupply_Lps <= 0 || designExtract_Lps <= 0)
            {
                refusal = string.Format(CultureInfo.InvariantCulture, "cannot be operated for a design of {0} / {1} l/s supply / extract.", designSupply_Lps, designExtract_Lps);
                return double.NaN;
            }

            //Never below the published minimum: TemplateRefusal holds the guidance figure inside the range, and the
            //result is never below the guidance figure.
            double design_Lps = System.Math.Max(designSupply_Lps, designExtract_Lps);
            double result = System.Math.Max(design_Lps, guidance_Lps);

            if (result > maximum_Lps)
            {
                refusal = string.Format(CultureInfo.InvariantCulture, "would cool at {0:0.###} l/s - the dwelling's design of {1:0.###} l/s - beyond the manufacturer's published cooling airflow range of {2:0.###}-{3:0.###} l/s, where no cooling data exists.", result, design_Lps, minimum_Lps, maximum_Lps);
                return double.NaN;
            }

            if (!(result <= ventilationUnitTemplate.MaximumSupplyFlowRate_Lps) || !(result <= ventilationUnitTemplate.MaximumExtractFlowRate_Lps))
            {
                refusal = string.Format(CultureInfo.InvariantCulture, "would cool at {0:0.###} l/s, beyond the unit's {1:0.###} / {2:0.###} l/s supply / extract capacity.", result, ventilationUnitTemplate.MaximumSupplyFlowRate_Lps, ventilationUnitTemplate.MaximumExtractFlowRate_Lps);
                return double.NaN;
            }

            return result;
        }

        /// <summary>
        /// A fingerprint of everything a product's manufacturer guidance says about how it cools: its identity and
        /// its whole operating strategy, source included. A change to any figure a cooled result was simulated with
        /// moves it, so the result is stale rather than silently kept.
        /// </summary>
        public static string PartOCoolingGuidanceFingerprint(this VentilationUnitTemplate ventilationUnitTemplate)
        {
            VentilationUnitReference reference = ventilationUnitTemplate?.VentilationUnitReference;

            List<string> lines =
            [
                string.Join("|", "product", reference?.Manufacturer ?? string.Empty, reference?.Model ?? string.Empty, reference?.Reference ?? string.Empty),
                string.Join("|", "capacity", FingerprintNumber(ventilationUnitTemplate?.MaximumSupplyFlowRate_Lps ?? double.NaN), FingerprintNumber(ventilationUnitTemplate?.MaximumExtractFlowRate_Lps ?? double.NaN)),
                "strategy|" + (ventilationUnitTemplate?.OperatingStrategy?.ToJsonObject()?.ToJsonString() ?? "none"),
            ];

            return PartOFingerprint("PartOCoolingGuidance:v1", lines);
        }

        /// <summary>
        /// The one product template <paramref name="ventilationUnitReference"/> identifies, or null where none or more than
        /// one does - an ambiguous product never has its cooling guidance guessed.
        /// </summary>
        public static VentilationUnitTemplate PartOCoolingTemplate(IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates, VentilationUnitReference ventilationUnitReference)
        {
            if (ventilationUnitTemplates is null || ventilationUnitReference is null || !ventilationUnitReference.IsValid)
            {
                return null;
            }

            VentilationUnitTemplate result = null;
            foreach (VentilationUnitTemplate ventilationUnitTemplate in ventilationUnitTemplates)
            {
                if (ventilationUnitTemplate?.VentilationUnitReference is null || !ventilationUnitTemplate.VentilationUnitReference.Matches(ventilationUnitReference))
                {
                    continue;
                }

                if (result is not null)
                {
                    return null;
                }

                result = ventilationUnitTemplate;
            }

            return result;
        }

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
