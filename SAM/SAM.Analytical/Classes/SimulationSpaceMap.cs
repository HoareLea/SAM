// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// Maps the spaces of a simulation-derived model back onto the design model they came from, so a result
    /// can be attributed to the design object that produced it.
    /// <para>
    /// <b>Why this exists.</b> A simulation model is a rebuild, not the design model: reading a TAS TSD
    /// produces fresh <c>Space</c> objects with fresh guids, and the results carry those. Attributing them
    /// by <b>name</b> - which is what the existing Grasshopper path does - works right up until two rooms
    /// share a name, and in a block of flats that is the normal case rather than the exotic one: every flat
    /// has a "Bedroom 2". Getting it wrong does not fail loudly; it silently reports one dwelling's
    /// overheating against another's, which is the worst class of error this workflow can make.
    /// </para>
    /// <para>
    /// <b>It never guesses.</b> Where an identity cannot be resolved unambiguously the space is reported as
    /// unresolved or ambiguous and no mapping is offered. A missing result is a visible gap; a confidently
    /// misattributed one is a wrong engineering answer that looks right.
    /// </para>
    /// <para>
    /// The stable key is supplied by the caller rather than named here, because the identity that survives a
    /// round trip is engine-specific - for TAS it is the zone guid stamped onto the design spaces by the
    /// workflow - and <c>SAM.Analytical</c> must not know about any engine. Name matching remains as the
    /// fallback for models that predate the stamp, but only where the name is unique on both sides.
    /// </para>
    /// </summary>
    public class SimulationSpaceMap
    {
        private readonly Dictionary<Guid, Space> dictionary_Design = [];

        /// <summary>
        /// The other direction: design space guid to the simulation space it produced. <b>Needed because a
        /// scenario names a DESIGN zone</b>, so carrying its intent forward - which ventilation strategy governs
        /// which simulated space - means walking design to simulation, not simulation to design.
        /// </summary>
        private readonly Dictionary<Guid, Space> dictionary_Simulation = [];

        private readonly List<Space> spaces_Unresolved = [];

        private readonly List<Space> spaces_Ambiguous = [];

        private readonly List<Space> spaces_Design_Ambiguous = [];

        /// <summary>
        /// Builds the map.
        /// </summary>
        /// <param name="spaces_Design">The spaces of the design model - the identities results must land on.</param>
        /// <param name="spaces_Simulation">The spaces of the model read back from the simulation.</param>
        /// <param name="func_StableKey">
        /// The engine-stable identity of a space, or null/blank where it has none. Applied to both sides. A
        /// caller with no such identity may pass null, which falls the whole map back to unique names.
        /// </param>
        public SimulationSpaceMap(IEnumerable<Space> spaces_Design, IEnumerable<Space> spaces_Simulation, Func<Space, string> func_StableKey = null)
        {
            List<Space> spaces_Design_Temp = Clean(spaces_Design);
            List<Space> spaces_Simulation_Temp = Clean(spaces_Simulation);

            Dictionary<string, List<Space>> dictionary_Key = Group(spaces_Design_Temp, func_StableKey);
            Dictionary<string, List<Space>> dictionary_Name = Group(spaces_Design_Temp, x => x.Name);

            foreach (Space space_Simulation in spaces_Simulation_Temp)
            {
                //The stable key first, and where it is present on both sides it is the whole answer: a name
                //disagreement then means the space was renamed, not that the match is wrong.
                string key = func_StableKey?.Invoke(space_Simulation);

                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (dictionary_Key.TryGetValue(key, out List<Space> spaces_Key))
                    {
                        if (spaces_Key.Count == 1)
                        {
                            Resolve(space_Simulation, spaces_Key[0]);
                            continue;
                        }

                        //One key on two design spaces is a broken model, not a naming coincidence. Say so
                        //rather than fall through to the weaker rule and appear to have resolved it.
                        spaces_Ambiguous.Add(space_Simulation);
                        continue;
                    }

                    //A STATED key that matches no design space is not the same thing as no key at all. It is
                    //evidence this space came from a different simulation batch than the design it is being
                    //paired with - e.g. results re-run against an updated model whose room names happened to
                    //stay the same while its stable identities changed. Falling through to the name match
                    //would then silently attribute a rerun's result to a room it never came from, which is
                    //exactly the misattribution this class exists to refuse. Name fallback is reserved for a
                    //space that carries NO identity at all.
                    spaces_Unresolved.Add(space_Simulation);
                    continue;
                }

                if (dictionary_Name.TryGetValue(space_Simulation.Name ?? string.Empty, out List<Space> spaces_Name))
                {
                    if (spaces_Name.Count == 1)
                    {
                        Resolve(space_Simulation, spaces_Name[0]);
                        continue;
                    }

                    //Two design spaces share this name - typically the same room in two flats. Refusing here
                    //is the entire point of this class.
                    spaces_Ambiguous.Add(space_Simulation);
                    continue;
                }

                spaces_Unresolved.Add(space_Simulation);
            }
        }

        /// <summary>
        /// A map over one set of spaces where each space <b>is</b> its own counterpart.
        /// <para>
        /// <b>For a design-only workflow, where there is no simulation to map to.</b> Preparing the TAS TM59
        /// configuration works on the design model: the spaces the export writes are the design spaces, so
        /// carrying a scenario to them needs an identity, not a translation. Passing a self-built map with a null
        /// key function would be wrong there - it would fall back to names and refuse three rooms all called
        /// "Bedroom 2", which are exactly the rooms that workflow has to export.
        /// </para>
        /// <para>
        /// Resolution is by <c>Guid</c> to itself, so duplicate names cannot make it ambiguous and no name is
        /// read at all. <see cref="IsComplete"/> is true for any set of non-null spaces.
        /// </para>
        /// </summary>
        public static SimulationSpaceMap Identity(IEnumerable<Space> spaces)
        {
            List<Space> spaces_Temp = Clean(spaces);

            //Both sides are the same objects, and the key is each space's own guid - which is unique by
            //construction, so neither the key branch nor the name branch can be ambiguous.
            return new SimulationSpaceMap(spaces_Temp, spaces_Temp, x => x.Guid.ToString());
        }

        /// <summary>
        /// Records one resolved pair, in both directions.
        /// <para>
        /// <b>The reverse direction has its own ambiguity, and it is not the same one.</b> Two simulation spaces
        /// can resolve to a single design space - one stable key stamped on two simulated zones, or two
        /// simulated spaces sharing a unique design name. Neither direction is then safe: backwards, "which
        /// simulated result belongs to this dwelling" has two answers; forwards, restoring the same design
        /// intent onto both candidates would let both produce apparently valid results. Both candidates and the
        /// design space are therefore struck out and reported.
        /// </para>
        /// </summary>
        private void Resolve(Space space_Simulation, Space space_Design)
        {
            if (dictionary_Simulation.TryGetValue(space_Design.Guid, out Space space_Simulation_Existing))
            {
                if (space_Simulation_Existing != null && space_Simulation_Existing.Guid == space_Simulation.Guid)
                {
                    //The same object was supplied twice. It is one mapping, not two candidates.
                    dictionary_Design[space_Simulation.Guid] = space_Design;

                    return;
                }

                if (space_Simulation_Existing != null)
                {
                    //Null marks both directions struck out rather than removing either entry, so a third
                    //claimant cannot silently re-resolve the design space and neither of the first two
                    //simulation spaces can still receive design intent through Design().
                    dictionary_Simulation[space_Design.Guid] = null;
                    dictionary_Design[space_Simulation_Existing.Guid] = null;
                    Add(spaces_Ambiguous, space_Simulation_Existing);
                    Add(spaces_Design_Ambiguous, space_Design);
                }

                dictionary_Design[space_Simulation.Guid] = null;
                Add(spaces_Ambiguous, space_Simulation);

                return;
            }

            dictionary_Design[space_Simulation.Guid] = space_Design;
            dictionary_Simulation[space_Design.Guid] = space_Simulation;
        }

        /// <summary>
        /// The design space a simulation space came from, or null where it could not be resolved
        /// unambiguously.
        /// </summary>
        public Space Design(Space space_Simulation)
        {
            return space_Simulation != null && dictionary_Design.TryGetValue(space_Simulation.Guid, out Space result) ? result : null;
        }

        /// <summary>
        /// The simulation space a design space produced, or null where nothing produced it or more than one
        /// thing did.
        /// <para>
        /// <b>This is the direction a scenario travels.</b> An <c>OverheatingScenario</c> names a design zone,
        /// so deciding which simulated spaces it governs - and therefore which TM59 criterion applies to them -
        /// means going design to simulation. Null is a refusal and must be treated as one: the alternative is
        /// matching the design space's name against the simulated model, which is the defect this class exists
        /// to remove.
        /// </para>
        /// </summary>
        public Space Simulation(Space space_Design)
        {
            return space_Design != null && dictionary_Simulation.TryGetValue(space_Design.Guid, out Space result) ? result : null;
        }

        /// <summary>Simulation spaces with no counterpart in the design model at all.</summary>
        public List<Space> Unresolved => [.. spaces_Unresolved];

        /// <summary>
        /// Simulation spaces whose counterpart could not be told apart from another - a shared name, or one
        /// stable key on two design spaces. Reported, never guessed at.
        /// </summary>
        public List<Space> Ambiguous => [.. spaces_Ambiguous];

        /// <summary>
        /// Design spaces that more than one simulation space resolved to. Reported and struck out of both
        /// <see cref="Simulation"/> and <see cref="Design"/>, never guessed between.
        /// </summary>
        public List<Space> AmbiguousDesign => [.. spaces_Design_Ambiguous];

        /// <summary>
        /// Whether every simulation space resolved to exactly one design space <b>and</b> no design space was
        /// claimed by two simulation spaces - so the mapping is one-to-one and usable in both directions.
        /// </summary>
        public bool IsComplete => spaces_Unresolved.Count == 0 && spaces_Ambiguous.Count == 0 && spaces_Design_Ambiguous.Count == 0;

        private static List<Space> Clean(IEnumerable<Space> spaces)
        {
            List<Space> result = [];

            foreach (Space space in spaces ?? [])
            {
                if (space != null)
                {
                    result.Add(space);
                }
            }

            return result;
        }

        private static Dictionary<string, List<Space>> Group(IEnumerable<Space> spaces, Func<Space, string> func)
        {
            Dictionary<string, List<Space>> result = [];

            if (func == null)
            {
                return result;
            }

            foreach (Space space in spaces)
            {
                string key = func.Invoke(space);
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (!result.TryGetValue(key, out List<Space> spaces_Key))
                {
                    spaces_Key = [];
                    result[key] = spaces_Key;
                }

                spaces_Key.Add(space);
            }

            return result;
        }

        private static void Add(List<Space> spaces, Space space)
        {
            if (space != null && spaces.Find(x => x != null && x.Guid == space.Guid) == null)
            {
                spaces.Add(space);
            }
        }
    }
}
