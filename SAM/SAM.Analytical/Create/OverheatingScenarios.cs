// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        /// <summary>
        /// One Approved Document O scenario per zone, at a stated mitigation stage - the whole set an iteration
        /// needs, with each zone's <b>scope</b> and the stage's <b>operating assumptions</b> filled in.
        /// <para>
        /// <b>Why a factory.</b> Building these by hand is where an iteration goes wrong. A caller has to know
        /// that a flat is <c>Dwelling</c> and a communal corridor is <c>CommonSpace</c> - and get it from the same
        /// rule the Part F calculation uses rather than from the zone's name - and has to populate the stage's
        /// operating assumptions identically every time, or two statements of the same assessment derive two
        /// different keys. Both of those are asked for here instead of restated per caller.
        /// </para>
        /// <para>
        /// <b>Scope comes from <c>Query.PartOClassifyAssessmentZones</c></b>, so a zone this calls a dwelling is
        /// exactly a zone Part F sizes, and the corridor is assessed in its own right rather than attributed to a
        /// flat. A zone that is neither - because it carries no dwelling marking at all - is <b>refused</b>, not
        /// guessed at: assessing a corridor against the dwelling criterion, or a flat against the corridor one,
        /// is a wrong answer that looks right.
        /// </para>
        /// <para>
        /// <b>What this does NOT do.</b> It states intent. It does not touch the simulation inputs, so nothing
        /// here makes a model's openings actually operate without restriction - see
        /// <see cref="Query.PartOOperatingAssumptions(PartOIteration, out string)"/> for why that boundary is
        /// where it is.
        /// </para>
        /// </summary>
        /// <param name="zones">
        /// The zones to assess - typically every zone of the dwelling category, flats and corridor together.
        /// </param>
        /// <param name="partOIteration">The mitigation stage every scenario in this set states.</param>
        /// <param name="dictionary_VentilationStrategy">
        /// The ventilation strategy each zone states, by zone guid - <c>NV</c>, <c>MV</c>, <c>MVRE</c>,
        /// <c>UV</c>. <b>Authoritative over the model's own data</b>, which is the point of step 7. A zone with
        /// no entry is refused rather than defaulted: a silent <c>"NV"</c> assessed an MVRE dwelling against the
        /// natural-ventilation criterion, and that is the defect this replaced.
        /// </param>
        /// <param name="refusals">Every zone that produced no scenario, and why. One sentence each.</param>
        public static List<OverheatingScenario> OverheatingScenarios(IEnumerable<Zone> zones, PartOIteration partOIteration, Dictionary<System.Guid, string> dictionary_VentilationStrategy, out List<string> refusals)
        {
            refusals = [];

            //Asked once for the whole set: a stage that is not characterised must not produce a set of scenarios
            //that look stated but assume nothing.
            OverheatingOperatingAssumptions overheatingOperatingAssumptions = partOIteration.PartOOperatingAssumptions(out string refusal_Iteration);
            if (overheatingOperatingAssumptions == null)
            {
                refusals.Add(refusal_Iteration);

                return [];
            }

            List<Zone> zones_Temp = [];
            foreach (Zone zone in zones ?? [])
            {
                if (zone != null)
                {
                    zones_Temp.Add(zone);
                }
            }

            if (zones_Temp.Count == 0)
            {
                refusals.Add("No zones were supplied, so there is nothing to assess.");

                return [];
            }

            //The Dwelling/CommonSpace split is asked for, not restated, so it cannot drift from what Part F sizes.
            zones_Temp.PartOClassifyAssessmentZones(out List<Zone> zones_Dwelling, out List<Zone> zones_CommonSpace);

            //One rule layered on top of that split, and it is stricter on purpose.
            //
            //PartOClassifyAssessmentZones calls everything that is not a dwelling a common space, which is right
            //for classifying a model that has been marked up. But an UNMARKED zone sitting beside marked ones is
            //not a common space - it is a zone the model declined to say anything about, and calling it one would
            //assess a bedroom against the corridor criterion. Refusing it names the gap instead.
            //
            //The exception is a model that marks NOTHING: there, PartFDwellingZones treats every zone as a
            //dwelling (a model predating the parameter must not size nothing at all), and that legacy behaviour is
            //preserved rather than turned into a refusal for the whole building.
            zones_Temp.PartFClassifyDwellingZones(out List<Zone> zones_Marked_Dwelling, out List<Zone> zones_Marked_NotDwelling, out List<Zone> zones_Unmarked);

            bool marked = zones_Marked_Dwelling.Count != 0 || zones_Marked_NotDwelling.Count != 0;

            List<OverheatingScenario> result = [];

            foreach (Zone zone in zones_Temp)
            {
                if (marked && zones_Unmarked.Find(x => x.Guid == zone.Guid) != null)
                {
                    refusals.Add(string.Format("Zone '{0}' is not marked as a dwelling or not a dwelling while other zones in the model are, so it produces no scenario. Set its 'Is Dwelling' parameter - assessing it as a common space would apply the corridor criterion to a room that may be a bedroom.", zone.Name));
                    continue;
                }

                //Identity, not name - two zones can be called the same thing.
                PartOAssessmentScope partOAssessmentScope = PartOAssessmentScope.Undefined;

                if (zones_Dwelling.Find(x => x.Guid == zone.Guid) != null)
                {
                    partOAssessmentScope = PartOAssessmentScope.Dwelling;
                }
                else if (zones_CommonSpace.Find(x => x.Guid == zone.Guid) != null)
                {
                    partOAssessmentScope = PartOAssessmentScope.CommonSpace;
                }

                if (partOAssessmentScope == PartOAssessmentScope.Undefined)
                {
                    refusals.Add(string.Format("Zone '{0}' could not be classified as a dwelling or a common space, so it produces no scenario.", zone.Name));
                    continue;
                }

                //The neutral common-space identity asserts nothing about dwelling ventilation, so stating it
                //for a dwelling would file a dwelling result under a key that says nothing about how the
                //dwelling was ventilated.
                if (partOIteration == PartOIteration.DwellingIndependent && partOAssessmentScope != PartOAssessmentScope.CommonSpace)
                {
                    refusals.Add(string.Format("Zone '{0}' is a dwelling, and the DwellingIndependent iteration is the neutral identity of a common space only, so it produces no scenario. A dwelling is assessed at the base iteration of its own ventilation route.", zone.Name));
                    continue;
                }

                string ventilationStrategy = null;
                if (dictionary_VentilationStrategy == null || !dictionary_VentilationStrategy.TryGetValue(zone.Guid, out ventilationStrategy) || string.IsNullOrWhiteSpace(ventilationStrategy))
                {
                    refusals.Add(string.Format("Zone '{0}' states no ventilation strategy, so it produces no scenario. A strategy is required rather than defaulted - a silent default assessed a mechanically ventilated dwelling against the natural-ventilation criterion.", zone.Name));
                    continue;
                }

                OverheatingScenario overheatingScenario = new(
                    partOAssessmentScope,
                    zone.Guid,
                    partOIteration,
                    new SystemTemplate(ventilationStrategy, null, null, null, null, null),
                    overheatingOperatingAssumptions)
                {
                    //Presentation only, and deliberately readable: this is what a user sees in Grasshopper when
                    //they are checking they built the iteration they meant to.
                    Name = string.Format("{0} - {1} - {2}", zone.Name, partOIteration, ventilationStrategy),
                };

                result.Add(overheatingScenario);
            }

            return result;
        }
    }
}
