// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Attributing a simulation result back to the design object that produced it.
    /// <para>
    /// The existing Grasshopper path matches simulation spaces to design spaces by NAME, and the real
    /// three-flat model has a "Bedroom 2" in more than one flat. Matching by name there does not fail
    /// loudly - it reports one dwelling's overheating against another's. These tests fix the rule that
    /// prevents it: resolve by stable identity, and where identity is ambiguous refuse rather than guess.
    /// </para>
    /// </summary>
    public class SimulationSpaceMapTests
    {
        private const string key = "TasZoneGuid";

        // ------------------------------------------------------------------
        // The failure this class exists to prevent
        // ------------------------------------------------------------------

        /// <summary>
        /// <b>Two flats each holding a "Bedroom 2" must never cross-attribute.</b> With no stable key the
        /// name is ambiguous, so neither resolves - the safe answer. A map that returned either one would
        /// have a 50% chance of reporting Flat 3's overheating against Flat 2.
        /// </summary>
        [Fact]
        public void SharedRoomName_IsRefusedRatherThanGuessed()
        {
            Space space_Design_Flat2 = Space("Bedroom 2", null);
            Space space_Design_Flat3 = Space("Bedroom 2", null);

            Space space_Simulation = Space("Bedroom 2", null);

            SimulationSpaceMap simulationSpaceMap = new([space_Design_Flat2, space_Design_Flat3], [space_Simulation], StableKey);

            Assert.Null(simulationSpaceMap.Design(space_Simulation));
            Assert.Equal([space_Simulation.Guid], simulationSpaceMap.Ambiguous.ConvertAll(x => x.Guid));
            Assert.False(simulationSpaceMap.IsComplete);
        }

        /// <summary>
        /// The same two rooms, now carrying the stable identity the workflow stamps on them, resolve
        /// correctly and to DIFFERENT design spaces. This is the case the pipeline is meant to be in.
        /// </summary>
        [Fact]
        public void SharedRoomName_IsResolvedByStableIdentity()
        {
            Space space_Design_Flat2 = Space("Bedroom 2", "zone-2");
            Space space_Design_Flat3 = Space("Bedroom 2", "zone-3");

            Space space_Simulation_Flat2 = Space("Bedroom 2", "zone-2");
            Space space_Simulation_Flat3 = Space("Bedroom 2", "zone-3");

            SimulationSpaceMap simulationSpaceMap = new(
                [space_Design_Flat2, space_Design_Flat3],
                [space_Simulation_Flat2, space_Simulation_Flat3],
                StableKey);

            Assert.Equal(space_Design_Flat2.Guid, simulationSpaceMap.Design(space_Simulation_Flat2).Guid);
            Assert.Equal(space_Design_Flat3.Guid, simulationSpaceMap.Design(space_Simulation_Flat3).Guid);
            Assert.True(simulationSpaceMap.IsComplete);
        }

        // ------------------------------------------------------------------
        // Identity beats name
        // ------------------------------------------------------------------

        /// <summary>
        /// A space renamed after the run still resolves, because the stable key decides. The name
        /// disagreeing means the room was renamed, not that the match is wrong.
        /// </summary>
        [Fact]
        public void RenamedSpace_StillResolvesByStableIdentity()
        {
            Space space_Design = Space("Bedroom 2 (renamed)", "zone-7");
            Space space_Simulation = Space("Bedroom 2", "zone-7");

            SimulationSpaceMap simulationSpaceMap = new([space_Design], [space_Simulation], StableKey);

            Assert.Equal(space_Design.Guid, simulationSpaceMap.Design(space_Simulation).Guid);
        }

        /// <summary>
        /// <b>A STATED key that matches no design space is refused, never given a second chance by name.</b>
        /// <para>
        /// This is the shape a rerun against an updated design takes: the simulated space still carries an
        /// identity, but that identity belonged to a design that has since changed - a different simulation
        /// batch, an updated model whose stable IDs moved on while its room names happened to stay the same.
        /// Falling through to a name match here would silently attribute that rerun's result to a room it
        /// never came from, which is the exact misattribution this class exists to refuse. The room in this
        /// fixture shares its name with no other design space, so a map that wrongly fell back to name
        /// matching would resolve it - the failure has to be caught here, not merely by an ambiguous name.
        /// </para>
        /// </summary>
        [Fact]
        public void AStatedKeyMatchingNoDesignSpace_IsUnresolvedNotNameMatched()
        {
            Space space_Design = Space("Bedroom 2", "zone-old");
            Space space_Simulation = Space("Bedroom 2", "zone-new");

            SimulationSpaceMap simulationSpaceMap = new([space_Design], [space_Simulation], StableKey);

            Assert.Null(simulationSpaceMap.Design(space_Simulation));
            Assert.Equal([space_Simulation.Guid], simulationSpaceMap.Unresolved.ConvertAll(x => x.Guid));
            Assert.Empty(simulationSpaceMap.Ambiguous);
            Assert.False(simulationSpaceMap.IsComplete);
        }

        /// <summary>
        /// <b>Identity beats a contradicting name - but only because the identity is unique.</b> The
        /// simulation space is called "Bedroom 2" and carries zone-7, while a different design space is
        /// genuinely called "Bedroom 2". The key wins and resolves to the Kitchen, because a renamed room is
        /// far likelier than a re-keyed one; the name is not consulted at all once a unique key matches.
        /// </summary>
        [Fact]
        public void StableIdentity_WinsOverAContradictingName()
        {
            Space space_Design_Keyed = Space("Kitchen_4", "zone-7");
            Space space_Design_Named = Space("Bedroom 2", "zone-9");

            Space space_Simulation = Space("Bedroom 2", "zone-7");

            SimulationSpaceMap simulationSpaceMap = new([space_Design_Keyed, space_Design_Named], [space_Simulation], StableKey);

            Assert.Same(space_Design_Keyed, simulationSpaceMap.Design(space_Simulation));
        }

        /// <summary>
        /// <b>The stable key must be unique per SPACE, not per zone or dwelling.</b> The TAS zone guid is
        /// verified as one-per-space on the real three-flat model, and this pins the consequence of that
        /// ever ceasing to hold: a key shared by the two spaces of one flat resolves nothing, rather than
        /// attributing both rooms' results to whichever space was seen first.
        /// <para>
        /// This is the check that stops one ambiguity problem being traded for another - a zone-level key
        /// would look like a stable identity and quietly collapse every room in a flat onto one.
        /// </para>
        /// </summary>
        [Fact]
        public void ZoneLevelKeySharedByTwoSpaces_IsRefused()
        {
            //Both rooms of Flat 2, keyed by the DWELLING rather than by the space.
            Space space_Design_Bedroom = Space("Bedroom 2_3", "flat-2");
            Space space_Design_Kitchen = Space("Kitchen_4", "flat-2");

            Space space_Simulation_Bedroom = Space("Bedroom 2_3", "flat-2");
            Space space_Simulation_Kitchen = Space("Kitchen_4", "flat-2");

            SimulationSpaceMap simulationSpaceMap = new(
                [space_Design_Bedroom, space_Design_Kitchen],
                [space_Simulation_Bedroom, space_Simulation_Kitchen],
                StableKey);

            Assert.Null(simulationSpaceMap.Design(space_Simulation_Bedroom));
            Assert.Null(simulationSpaceMap.Design(space_Simulation_Kitchen));
            Assert.Equal(2, simulationSpaceMap.Ambiguous.Count);
        }

        /// <summary>
        /// One stable key on two design spaces is a broken model. It is reported as ambiguous rather than
        /// quietly falling through to the weaker name rule, which would look like a successful match.
        /// </summary>
        [Fact]
        public void DuplicateStableIdentity_IsAmbiguousAndDoesNotFallBackToName()
        {
            Space space_Design_1 = Space("Kitchen", "zone-dup");
            Space space_Design_2 = Space("Bathroom", "zone-dup");

            Space space_Simulation = Space("Kitchen", "zone-dup");

            SimulationSpaceMap simulationSpaceMap = new([space_Design_1, space_Design_2], [space_Simulation], StableKey);

            Assert.Null(simulationSpaceMap.Design(space_Simulation));
            Assert.Single(simulationSpaceMap.Ambiguous);
        }

        // ------------------------------------------------------------------
        // The legacy path stays usable
        // ------------------------------------------------------------------

        /// <summary>
        /// A model with no stable identity anywhere still maps, provided each name is unique - which is how
        /// models that predate the stamp keep working.
        /// </summary>
        [Fact]
        public void UniqueNames_ResolveWithoutAnyStableIdentity()
        {
            Space space_Design = Space("Studio 1_0", null);
            Space space_Simulation = Space("Studio 1_0", null);

            SimulationSpaceMap simulationSpaceMap = new([space_Design], [space_Simulation], StableKey);

            Assert.Equal(space_Design.Guid, simulationSpaceMap.Design(space_Simulation).Guid);
            Assert.True(simulationSpaceMap.IsComplete);
        }

        /// <summary>A simulation space with no design counterpart is reported, not silently dropped.</summary>
        [Fact]
        public void SpaceWithNoCounterpart_IsUnresolved()
        {
            Space space_Simulation = Space("Plant Room", "zone-99");

            SimulationSpaceMap simulationSpaceMap = new([Space("Studio 1_0", "zone-1")], [space_Simulation], StableKey);

            Assert.Null(simulationSpaceMap.Design(space_Simulation));
            Assert.Equal([space_Simulation.Guid], simulationSpaceMap.Unresolved.ConvertAll(x => x.Guid));
            Assert.Empty(simulationSpaceMap.Ambiguous);
        }

        /// <summary>
        /// The design space is returned by identity, never by the simulation space's own guid - a rebuilt
        /// model has fresh guids and they must not leak into an association.
        /// </summary>
        [Fact]
        public void ResolvedDesignSpace_IsTheDesignInstance()
        {
            Space space_Design = Space("Kitchen_4", "zone-4");
            Space space_Simulation = Space("Kitchen_4", "zone-4");

            Assert.NotEqual(space_Design.Guid, space_Simulation.Guid);

            SimulationSpaceMap simulationSpaceMap = new([space_Design], [space_Simulation], StableKey);

            Assert.Same(space_Design, simulationSpaceMap.Design(space_Simulation));
        }

        [Fact]
        public void NoSpaces_IsCompleteAndEmpty()
        {
            SimulationSpaceMap simulationSpaceMap = new(null, null, StableKey);

            Assert.True(simulationSpaceMap.IsComplete);
            Assert.Empty(simulationSpaceMap.Unresolved);
            Assert.Empty(simulationSpaceMap.Ambiguous);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Stands in for the engine-stable identity the caller supplies - for TAS, the zone guid the
        /// workflow stamps onto the design spaces.
        /// </summary>
        private static string StableKey(Space space)
        {
            return space != null && space.TryGetValue(key, out string result) ? result : null;
        }

        private static Space Space(string name, string stableKey)
        {
            Space result = new(name);

            if (!string.IsNullOrWhiteSpace(stableKey))
            {
                result.SetValue(key, stableKey);
            }

            return result;
        }
    }
}
