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
        /// The Approved Document O ventilation route a stated ventilation strategy means, or
        /// <see cref="PartOVentilationMode.Undefined"/> with a refusal where it means none.
        /// <para>
        /// <b>This is a total, explicit mapping and not a heuristic.</b> Every word that has a Part O route
        /// is listed; everything else refuses, including words that are perfectly valid elsewhere in SAM.
        /// Being a recognised <c>SystemTemplate</c> ventilation strategy, or being recognised by
        /// <see cref="VentilationStrategyMap"/> for the purpose of choosing a TM59 criterion, does not make
        /// a word a Part O route - those are different questions with different consequences.
        /// </para>
        /// <para>
        /// <b>Why the refusals are not conservatism.</b> The thing that is applied on the mechanical route
        /// is <c>PartFCalculator</c>'s sizing, which is unconditionally System 4 shaped: paragraph 1.67
        /// gives every habitable room a mechanical supply terminal. Writing that onto a dwelling whose
        /// route was never stated, or was stated only as "mechanical", simulates a building nobody
        /// described - successfully, and with nothing downstream saying so. A refusal is recoverable; a
        /// plausible wrong answer is not.
        /// </para>
        /// </summary>
        /// <param name="ventilationStrategy">
        /// The strategy as stated - <c>NV</c>, <c>MVHR</c>, <c>MVRE</c>, or one of the longer semantic
        /// spellings. Read trimmed and upper-cased with <c>ToUpperInvariant</c>, because it arrives off a
        /// Grasshopper text panel rather than through <c>SystemTemplate</c>'s space-stripping setters, and
        /// the current culture's casing rules would otherwise map <c>"nv"</c> to something that is not
        /// <c>"NV"</c>.
        /// </param>
        /// <param name="refusal">Why it means no route, or null where it means one.</param>
        public static PartOVentilationMode PartOVentilationMode(string ventilationStrategy, out string refusal)
        {
            refusal = null;

            if (string.IsNullOrWhiteSpace(ventilationStrategy))
            {
                refusal = "No Part O ventilation route was stated. State NaturalVentilation (Iteration 1b) or MVHR (Iteration 1a) - it is not defaulted, because an absent route read as mechanical writes Approved Document F System 4 supply and extract into a dwelling nobody said had them.";

                return Enums.PartOVentilationMode.Undefined;
            }

            switch (ventilationStrategy.Trim().ToUpperInvariant())
            {
                case "NV":
                case "NATURALVENTILATION":
                case "NATURAL VENTILATION":
                case "BASENATURALVENTILATION":
                    return Enums.PartOVentilationMode.NaturalVentilation;

                //MVRE is the word this codebase's models, fixtures and licensed acceptance runs use for the
                //heat-recovery arrangement Part F sizes; MVHR is the same route said the way the regulation
                //says it. Both are the one route, deliberately, rather than one being quietly preferred and
                //the other quietly refused.
                case "MVHR":
                case "MVRE":
                case "BASEMVHR":
                    return Enums.PartOVentilationMode.MVHR;

                //Named individually, because "mechanical ventilation" is not a Part O route. System 3
                //(continuous mechanical extract) and System 4 (continuous supply and extract with heat
                //recovery) are different buildings, and only System 4 is what PartFCalculator sizes.
                case "MV":
                    refusal = string.Format("'{0}' states that the dwelling is mechanically ventilated but not which mechanical route, so it is not a Part O ventilation route. Approved Document F System 3 - continuous mechanical extract - and System 4 - continuous supply and extract with heat recovery - are different buildings, and the Part F sizing SAM applies is System 4 shaped: it puts a mechanical supply terminal in every habitable room. State MVHR where that is the route.", ventilationStrategy.Trim());

                    return Enums.PartOVentilationMode.Undefined;

                //UV selects the TM59 corridor criterion. That is a statement about which criterion an
                //unoccupied common space is assessed against, not a statement that a dwelling has an MVHR
                //system, and it must not be one of the words that reaches the airflow application.
                case "UV":
                    refusal = string.Format("'{0}' selects the TM59 corridor criterion for a common space; it states no Part O ventilation route for a dwelling. Common spaces are not covered by Iteration 1a or Iteration 1b - assess them separately.", ventilationStrategy.Trim());

                    return Enums.PartOVentilationMode.Undefined;

                default:
                    refusal = string.Format("'{0}' is not a Part O ventilation route. The routes are NaturalVentilation (Iteration 1b) and MVHR (Iteration 1a). An unrecognised word is not read as mechanical - that rule is what put Approved Document F System 4 supply and extract into naturally ventilated dwellings.", ventilationStrategy.Trim());

                    return Enums.PartOVentilationMode.Undefined;
            }
        }

        /// <summary>
        /// The one Approved Document O ventilation route the assessed zones state, or
        /// <see cref="PartOVentilationMode.Undefined"/> with a refusal where they do not state exactly one.
        /// <para>
        /// <b>Whole-model, because the application is whole-model.</b>
        /// <see cref="Modify.ApplyPartFVentilationRates(AnalyticalModel, PartFOperatingMode, out List{string}, out List{string})"/>
        /// writes to every sized space at once, so a model whose zones state different routes has no
        /// correct answer available here: applying puts mechanical supply and extract into the naturally
        /// ventilated zones, and skipping strips the mechanical ones of the rates they are sized for. Both
        /// halves would be simulated as something they are not, so neither is done. Per-zone application is
        /// a separate change with its own transfer-air and balance consequences.
        /// </para>
        /// <para>
        /// <b>No assessed zones is an unstated route, not a default.</b> Nothing states how a model with no
        /// zones is ventilated, and the old behaviour - keep applying, because that is what this
        /// preparation always did - is exactly the silent System 4 write this type exists to stop.
        /// </para>
        /// </summary>
        /// <param name="zones">The zones the iteration assesses. Their guids key the strategies.</param>
        /// <param name="dictionary_VentilationStrategy">
        /// The route each assessed zone states, by zone guid - the same dictionary
        /// <see cref="Create.OverheatingScenarios(IEnumerable{Zone}, PartOIteration, Dictionary{Guid, string}, out List{string})"/>
        /// is given, so the airflow decision and the assessment criterion can never read different answers.
        /// </param>
        /// <param name="refusal">
        /// Why no single route was settled - naming every zone and what it stated - or null where one was.
        /// </param>
        public static PartOVentilationMode PartOVentilationMode(IEnumerable<Zone> zones, Dictionary<Guid, string> dictionary_VentilationStrategy, out string refusal)
        {
            refusal = null;

            List<Zone> zones_Assessed = [];

            foreach (Zone zone in zones ?? [])
            {
                if (zone != null)
                {
                    zones_Assessed.Add(zone);
                }
            }

            if (zones_Assessed.Count == 0)
            {
                refusal = "No zone was assessed, so nothing states a Part O ventilation route. The route is not defaulted: an absence read as mechanical writes Approved Document F System 4 supply and extract onto every sized space in the model. Zone the model and state NaturalVentilation or MVHR.";

                return Enums.PartOVentilationMode.Undefined;
            }

            List<string> descriptions_Refused = [];
            List<string> descriptions_Stated = [];

            PartOVentilationMode result = Enums.PartOVentilationMode.Undefined;
            bool mixed = false;

            foreach (Zone zone in zones_Assessed)
            {
                string ventilationStrategy = null;
                dictionary_VentilationStrategy?.TryGetValue(zone.Guid, out ventilationStrategy);

                PartOVentilationMode partOVentilationMode = PartOVentilationMode(ventilationStrategy, out string refusal_Zone);

                if (partOVentilationMode == Enums.PartOVentilationMode.Undefined)
                {
                    //Every zone is still visited, so one unstated zone reports one reason rather than hiding
                    //the others behind it.
                    descriptions_Refused.Add(string.Format("'{0}': {1}", zone.Name, refusal_Zone));

                    continue;
                }

                descriptions_Stated.Add(string.Format("'{0}' states {1}", zone.Name, partOVentilationMode));

                if (result == Enums.PartOVentilationMode.Undefined)
                {
                    result = partOVentilationMode;
                }
                else if (result != partOVentilationMode)
                {
                    mixed = true;
                }
            }

            if (descriptions_Refused.Count != 0)
            {
                refusal = string.Format("No Part O ventilation route was settled for {0} of the {1} assessed zone(s), so this iteration was not prepared. {2}", descriptions_Refused.Count, zones_Assessed.Count, string.Join(" ", descriptions_Refused));

                return Enums.PartOVentilationMode.Undefined;
            }

            if (mixed)
            {
                refusal = string.Format("The assessed zones state more than one Part O ventilation route, so this iteration was not prepared: {0}. Approved Document F airflow is applied to the whole model at once, so there is no answer here that is right for both halves - applying would put continuous mechanical supply and extract into the naturally ventilated zones, and skipping would strip the rates the MVHR zones are sized for. Prepare the naturally ventilated zones and the MVHR zones as separate iterations, or wait for the per-zone application.", string.Join(", ", descriptions_Stated));

                return Enums.PartOVentilationMode.Undefined;
            }

            return result;
        }
    }
}
