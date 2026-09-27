// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Carries the Approved Document F sized airflows onto the <b>internal conditions the simulation actually
        /// reads</b>, at one stated operating condition.
        /// <para>
        /// <b>Why this exists: the two representations were never connected.</b> <c>PartFCalculator</c> writes its
        /// sizing onto each space as <c>SpaceParameter.PartFSpaceData</c>, in litres per second, per terminal.
        /// The simulation reads airflow through <see cref="Query.CalculatedSupplyAirFlow(Space)"/>, in cubic metres
        /// per second, off the space's <c>InternalCondition</c> - and that query has never looked at
        /// <c>PartFSpaceData</c>. So a Part-F-sized model simulated with whatever its internal conditions happened
        /// to say, and the Part F numbers were reporting only. This is the bridge, and it is the only new arithmetic
        /// involved: the rates themselves are read from the terminals, never re-derived.
        /// </para>
        /// <para>
        /// <b>Part F becomes authoritative, and what it displaces is reported.</b>
        /// <see cref="Query.CalculatedSupplyAirFlow(Space)"/> <b>sums</b> four bases -
        /// <c>SupplyAirFlow</c>, <c>SupplyAirFlowPerPerson</c>, <c>SupplyAirFlowPerArea</c> and
        /// <c>SupplyAirChangesPerHour</c> - so writing a Part F rate alongside an existing per-area or per-person
        /// rate would silently <i>add</i> to it and over-ventilate the room. The other three bases are therefore
        /// cleared, and every space where something was cleared is named in <paramref name="notes"/> rather than
        /// changed quietly.
        /// </para>
        /// <para>
        /// <b>Each sized space gets its OWN internal condition.</b> Part F rates are per room, but internal
        /// conditions are routinely shared between rooms - so writing in place would have one bedroom's rate
        /// overwrite another's, and in a block of flats that is the normal case rather than the exotic one. A clone
        /// with a fresh guid is taken per space, named after the space so it is identifiable in TAS, and
        /// disambiguated where two rooms share a name. The original internal conditions are untouched.
        /// </para>
        /// <para>
        /// <b>The naming is idempotent.</b> The generated name is <c>&lt;condition&gt; - &lt;space&gt;</c>, and a
        /// condition whose name this method already generated <i>for the same space</i> - one or more suffixes
        /// deep, with or without the disambiguating <c>" (n)"</c> - is first stripped back to its base, so
        /// re-applying the rates (re-preparing a prepared model, or an Iteration 2B optimisation re-preparing
        /// every round) produces the SAME name again rather than appending the space name each time. The
        /// multi-pass workflow relies on this: baseline conversion, restoration and every optimisation round all
        /// run this method over the previous one's output.
        /// </para>
        /// <para>
        /// <b>Both directions are written, including zero.</b> A wet room sized for extract only gets
        /// <c>ExhaustAirFlow</c> and an explicit <c>SupplyAirFlow</c> of zero: under the balanced MVHR arrangement
        /// Part F sizes, its make-up air arrives as transfer air through the internal door, not as supply. Leaving
        /// a stale supply rate there would model a room that is ventilated twice.
        /// </para>
        /// <para>
        /// <b>This does not size anything and does not decide anything.</b> Run the Part F calculation first -
        /// <c>SAMAnalytical.AddVentilationPropertiesByPartF</c> or <c>SAMAnalytical.CheckPartFCompliance</c> - and
        /// this applies its answer. A space with no <c>PartFSpaceData</c> is left exactly as it was.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel">
        /// A model whose spaces already carry <c>PartFSpaceData</c>. <b>Not modified</b> - an updated copy is
        /// returned, matching the Part F sizing components.
        /// </param>
        /// <param name="partFOperatingMode">
        /// Which condition's rates to apply. <see cref="PartFOperatingMode.ContinuousDesign"/> is the Approved
        /// Document F sizing case and the one a base-provision assessment runs at.
        /// <see cref="PartFOperatingMode.MeasuredCommissioning"/> is <b>refused</b>: measured rates are
        /// commissioning evidence and must never be driven into a design simulation as though they were design
        /// values.
        /// </param>
        /// <param name="refusals">Why a space received no rates, one sentence each.</param>
        /// <param name="notes">
        /// What was applied and what was displaced - the record that makes this inspectable rather than magical.
        /// </param>
        /// <returns>The updated model, or null where nothing could be applied at all.</returns>
        public static AnalyticalModel ApplyPartFVentilationRates(this AnalyticalModel analyticalModel, PartFOperatingMode partFOperatingMode, out List<string> refusals, out List<string> notes)
        {
            return ApplyPartFVentilationRates(analyticalModel, partFOperatingMode, null, out refusals, out notes);
        }

        /// <summary>
        /// <see cref="ApplyPartFVentilationRates(AnalyticalModel, PartFOperatingMode, out List{string}, out List{string})"/>
        /// <b>scoped to the given spaces</b> - the seam a mixed model needs.
        /// <para>
        /// The whole-model call writes System 4 rates onto every sized space, including a naturally ventilated
        /// dwelling and an unassessed one beside the MVHR dwelling being prepared (PR0 P1, P11), and that rewrite
        /// is irreversible. Scoped, a space outside <paramref name="spaces"/> is treated exactly as an unsized
        /// space: its internal condition is not rewritten, not refused, and its name is reserved so a generated
        /// name cannot collide with it. Null keeps the whole-model behaviour bit for bit.
        /// </para>
        /// </summary>
        /// <param name="spaces">The spaces whose rates to apply, by identity. Null means every space.</param>
        public static AnalyticalModel ApplyPartFVentilationRates(this AnalyticalModel analyticalModel, PartFOperatingMode partFOperatingMode, IEnumerable<Space> spaces, out List<string> refusals, out List<string> notes)
        {
            refusals = [];
            notes = [];

            if (analyticalModel == null)
            {
                refusals.Add("No analytical model was supplied.");

                return null;
            }

            if (partFOperatingMode == PartFOperatingMode.MeasuredCommissioning)
            {
                refusals.Add("Measured commissioning rates are evidence recorded from site, not design inputs, so they are never applied to a simulation. Apply ContinuousDesign, HighBoost or Setback.");

                return null;
            }

            //The scope, by identity. Null keeps every space in scope - the whole-model behaviour.
            HashSet<Guid> guids_Scope = null;
            if (spaces != null)
            {
                guids_Scope = [];
                foreach (Space space_Scope in spaces)
                {
                    if (space_Scope != null)
                    {
                        guids_Scope.Add(space_Scope.Guid);
                    }
                }
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<Space> spaces_Cluster = adjacencyCluster?.GetSpaces();
            if (spaces_Cluster == null || spaces_Cluster.Count == 0)
            {
                refusals.Add("The model carries no spaces, so there are no Part F rates to apply.");

                return null;
            }

            //Decided ONCE per space, so the name reservation below and the application agree exactly on which
            //spaces this call rewrites: a space carrying Part F data, with a rate applicable at the stated
            //condition, and an internal condition to write it to.
            List<Space> spaces_Sized = [];
            List<double?> supply_Sized = [];
            List<double?> extract_Sized = [];

            //Internal condition names have to stay distinguishable in TAS, and neither the space name nor the
            //original condition name is unique - three flats each have a "Bedroom 2".
            HashSet<string> names = [];

            foreach (Space space in spaces_Cluster)
            {
                if (space == null)
                {
                    continue;
                }

                //Out of scope is exactly "not sized here": the condition survives this call, so its name is
                //reserved below like any other surviving name, and nothing is refused about it.
                PartFSpaceData partFSpaceData = guids_Scope != null && !guids_Scope.Contains(space.Guid) ? null : space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

                double? supply_Lps = partFSpaceData == null ? null : SupplyFlowRate_Lps(partFSpaceData, partFOperatingMode);
                double? extract_Lps = partFSpaceData == null ? null : ExtractFlowRate_Lps(partFSpaceData, partFOperatingMode);

                if (partFSpaceData != null && (supply_Lps.HasValue || extract_Lps.HasValue) && space.InternalCondition != null)
                {
                    spaces_Sized.Add(space);
                    supply_Sized.Add(supply_Lps);
                    extract_Sized.Add(extract_Lps);

                    continue;
                }

                if (partFSpaceData != null)
                {
                    if (!supply_Lps.HasValue && !extract_Lps.HasValue)
                    {
                        refusals.Add(string.Format("Space '{0}' carries Part F data but no {1} rate on either direction, so nothing was applied to it.", space.Name, Core.Query.Description(partFOperatingMode)));
                    }
                    else if (space.InternalCondition == null)
                    {
                        //Without an internal condition there is nowhere the simulation would read a rate from, and
                        //inventing one here would invent occupancy, gains and setpoints with it.
                        refusals.Add(string.Format("Space '{0}' has no internal condition, so its Part F rates have nowhere to go. Assign an internal condition before applying them.", space.Name));
                    }
                }

                //The set is seeded from every internal-condition name that SURVIVES this call - that is, from
                //the spaces it will NOT rewrite: an unsized space (legitimately so - circulation, storage, a
                //plant room), or one of the refusals above. An untouched condition whose name happens to match
                //the generated "<condition> - <space>" pattern must reserve that name: TAS identifies an
                //internal condition by name, so letting a generated condition collide with an existing one
                //would associate one room with another room's gains and airflow.
                //
                //A name this call is about to REPLACE is deliberately not reserved: the generated name is
                //stable across re-application (see UniqueName), so the replacement is meant to carry the same
                //name the outgoing condition had. Reserving the outgoing names would push every re-prepared
                //space to the next disambiguation index - " (2)", " (3)", ... - on every round.
                string name_Existing = space.InternalCondition?.Name;
                if (!string.IsNullOrWhiteSpace(name_Existing))
                {
                    names.Add(name_Existing);
                }
            }

            int count = 0;

            for (int i = 0; i < spaces_Sized.Count; i++)
            {
                Space space = spaces_Sized[i];

                double? supply_Lps = supply_Sized[i];
                double? extract_Lps = extract_Sized[i];

                InternalCondition internalCondition = space.InternalCondition;

                //A clone per space, with a fresh guid, so one room's rate cannot land on another's.
                InternalCondition internalCondition_Space = new(UniqueName(names, internalCondition.Name, space.Name), Guid.NewGuid(), internalCondition);

                List<string> cleared = Clear(internalCondition_Space);

                double supply = (supply_Lps ?? 0) / 1000.0;
                double extract = (extract_Lps ?? 0) / 1000.0;

                internalCondition_Space.SetValue(InternalConditionParameter.SupplyAirFlow, supply);
                internalCondition_Space.SetValue(InternalConditionParameter.ExhaustAirFlow, extract);

                //A COPY of the space, not the space itself. AdjacencyCluster's clone shares its Space objects with
                //the model it came from, so assigning the condition in place would reach back through the clone and
                //change the caller's model - which the "an updated copy is returned" contract forbids, and which a
                //caller comparing before and after would never see coming. The copy keeps its guid, so AddObject
                //replaces rather than duplicating.
                Space space_Applied = new(space)
                {
                    InternalCondition = internalCondition_Space,
                };

                adjacencyCluster.AddObject(space_Applied);

                count++;

                notes.Add(string.Format(
                    "Space '{0}': supply {1:0.###} m3/s ({2:0.###} l/s), extract {3:0.###} m3/s ({4:0.###} l/s) at the {5} condition{6}.",
                    space.Name,
                    supply,
                    supply_Lps ?? 0,
                    extract,
                    extract_Lps ?? 0,
                    Core.Query.Description(partFOperatingMode),
                    cleared.Count == 0 ? string.Empty : string.Format(", replacing {0}", string.Join(", ", cleared))));
            }

            if (count == 0)
            {
                refusals.Add("No space carried Part F data with an applicable rate. Run the Part F calculation first - SAMAnalytical.AddVentilationPropertiesByPartF or SAMAnalytical.CheckPartFCompliance.");

                return null;
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        /// <summary>
        /// Clears the airflow bases that would otherwise <b>add</b> to the Part F rate, and names the ones that
        /// were actually carrying a value.
        /// <para>
        /// Both directions, because <c>CalculatedSupplyAirFlow</c> has an exhaust twin and the same summing
        /// applies to it.
        /// </para>
        /// </summary>
        private static List<string> Clear(InternalCondition internalCondition)
        {
            List<string> result = [];

            InternalConditionParameter[] internalConditionParameters =
            [
                InternalConditionParameter.SupplyAirFlowPerPerson,
                InternalConditionParameter.SupplyAirFlowPerArea,
                InternalConditionParameter.SupplyAirChangesPerHour,
                InternalConditionParameter.ExhaustAirFlowPerPerson,
                InternalConditionParameter.ExhaustAirFlowPerArea,
                InternalConditionParameter.ExhaustAirChangesPerHour,
            ];

            foreach (InternalConditionParameter internalConditionParameter in internalConditionParameters)
            {
                if (internalCondition.TryGetValue(internalConditionParameter, out double value) && !double.IsNaN(value) && value != 0)
                {
                    result.Add(string.Format("{0} {1:0.###}", Core.Query.Name(internalConditionParameter), value));
                }

                //Set to zero rather than removed: a consumer that reads the parameter unconditionally then sees a
                //stated zero instead of an absence, and the summing query treats both the same way.
                internalCondition.SetValue(internalConditionParameter, 0.0);
            }

            return result;
        }

        /// <summary>Supply into the space at the stated condition, or null where it has no supply terminal.</summary>
        private static double? SupplyFlowRate_Lps(PartFSpaceData partFSpaceData, PartFOperatingMode partFOperatingMode)
        {
            switch (partFOperatingMode)
            {
                case PartFOperatingMode.ContinuousDesign:
                    return partFSpaceData.ContinuousSupplyFlowRate_Lps;

                case PartFOperatingMode.HighBoost:
                    return partFSpaceData.HighSupplyFlowRate_Lps;

                case PartFOperatingMode.Setback:
                    return partFSpaceData.SetbackSupplyFlowRate_Lps;

                default:
                    return null;
            }
        }

        /// <summary>Extract out of the space at the stated condition, or null where it has no extract terminal.</summary>
        private static double? ExtractFlowRate_Lps(PartFSpaceData partFSpaceData, PartFOperatingMode partFOperatingMode)
        {
            switch (partFOperatingMode)
            {
                case PartFOperatingMode.ContinuousDesign:
                    return partFSpaceData.ContinuousExtractFlowRate_Lps;

                case PartFOperatingMode.HighBoost:
                    return partFSpaceData.HighExtractFlowRate_Lps;

                case PartFOperatingMode.Setback:
                    return partFSpaceData.SetbackExtractFlowRate_Lps;

                default:
                    return null;
            }
        }

        /// <summary>
        /// The name for one sized space's own condition: <c>&lt;condition&gt; - &lt;space&gt;</c>,
        /// disambiguated as <c>... (n)</c> where that is already taken.
        /// <para>
        /// <b>Idempotent over re-application.</b> A condition whose name this method already generated
        /// <i>for this same space</i> - the suffix applied one or more times, with or without the
        /// disambiguating <c>" (n)"</c> - is first stripped back to its base, so re-running the preparation
        /// over a prepared model (as every Iteration 2B round does) yields the SAME name again rather than
        /// <c>"Studio - Studio 1_0 - Studio 1_0 - ..."</c>, which is the defect this guards: the name must
        /// identify the room in TAS, and each pass was lengthening it instead. The suffix is stripped only
        /// where it names THIS space, so a condition authored for another room is never rewritten toward it.
        /// </para>
        /// </summary>
        private static string UniqueName(HashSet<string> names, string name_InternalCondition, string name_Space)
        {
            string suffix = string.Format(" - {0}", name_Space);

            string name_Base = name_InternalCondition ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(name_Space))
            {
                //The disambiguation this method adds on a collision: "<base> - <space> (n)". Recognised only
                //where removing it leaves a name carrying THIS space's suffix - an authored name that merely
                //ends in a parenthesis is not this method's product and is not stripped here.
                if (name_Base.EndsWith(")", StringComparison.Ordinal))
                {
                    int index_Disambiguation = name_Base.LastIndexOf(" (", StringComparison.Ordinal);
                    if (index_Disambiguation > 0
                        && int.TryParse(name_Base.Substring(index_Disambiguation + 2, name_Base.Length - index_Disambiguation - 3), out int _)
                        && name_Base.Substring(0, index_Disambiguation).EndsWith(suffix, StringComparison.Ordinal))
                    {
                        name_Base = name_Base.Substring(0, index_Disambiguation);
                    }
                }

                //Every trailing copy of this space's own suffix, so a name already produced for this space -
                //once, or several times deep from the passes this guard was added to stop - collapses back to
                //its base before the suffix is applied exactly once below.
                while (name_Base.EndsWith(suffix, StringComparison.Ordinal))
                {
                    name_Base = name_Base.Substring(0, name_Base.Length - suffix.Length);
                }
            }

            string result = string.Format("{0}{1}", name_Base, suffix);

            if (names.Add(result))
            {
                return result;
            }

            //Two rooms with the same name on the same internal condition. Numbered rather than allowed to
            //collide, because TAS identifies an internal condition by name.
            int index = 2;
            while (!names.Add(string.Format("{0} ({1})", result, index)))
            {
                index++;
            }

            return string.Format("{0} ({1})", result, index);
        }
    }
}
