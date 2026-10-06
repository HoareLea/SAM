// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// What a ventilation system has to be able to move, summed from the design terminals connected
        /// to it.
        /// <para>
        /// <b>Derived, never stored.</b> A duty written onto the system would be a second answer that
        /// goes stale the moment a terminal is added, removed or re-balanced - and subdividing terminals
        /// is exactly what the design stage does. Asking the terminals every time is the only way the
        /// answer cannot be wrong.
        /// </para>
        /// <para>
        /// <b>Supply and extract are reported separately and are not required to be equal.</b> A balanced
        /// heat recovery system balances at the <i>system</i>, not in each room: a bedroom is supplied and
        /// not extracted, a bathroom is extracted and not supplied, and the air moves between them as
        /// transfer air. Forcing them equal per room is the mistake that would put an extract terminal in
        /// every bedroom.
        /// </para>
        /// <para>
        /// This is the magnitude half of what Iteration 2 needs. <see cref="PartFSystemCapabilityRequirement"/>
        /// already states what a system must be <i>able</i> to do; this states how much air it has to move
        /// while doing it.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model.</param>
        /// <param name="ventilationSystem">The system whose connected terminals are summed.</param>
        /// <param name="supplyDuty_Lps">Total design supply duty [l/s]. Zero where the system has no supply terminal.</param>
        /// <param name="extractDuty_Lps">Total design extract duty [l/s]. Zero where the system has no extract terminal.</param>
        /// <returns>Whether any terminal at all was found to sum.</returns>
        public static bool VentilationSystemDesignDuty(this AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, out double supplyDuty_Lps, out double extractDuty_Lps)
        {
            supplyDuty_Lps = 0;
            extractDuty_Lps = 0;

            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.VentilationTerminals(ventilationSystem);
            if (ventilationTerminals is null || ventilationTerminals.Count == 0)
            {
                return false;
            }

            supplyDuty_Lps = ventilationTerminals.VentilationTerminalDesignDuty_Lps(FlowClassification.Supply) ?? 0;
            extractDuty_Lps = ventilationTerminals.VentilationTerminalDesignDuty_Lps(FlowClassification.Extract) ?? 0;

            return true;
        }

        /// <summary>
        /// Checks the design duty of a system's terminals against the Approved Document F requirement its
        /// spaces carry, and says where they differ.
        /// <para>
        /// <b>Two independent derivations, compared rather than reconciled.</b> The duty comes from the
        /// design terminals; the requirement comes from <c>PartFSpaceData</c> on the spaces, which is what
        /// <c>PartFCalculator</c> wrote and what <c>Modify.ApplyPartFVentilationRates</c> read. Neither is
        /// allowed to correct the other.
        /// </para>
        /// <para>
        /// <b>The comparison is one-sided, and that is the Iteration 2 invariant.</b> A design duty
        /// <i>below</i> the requirement at the system total is a <b>refusal</b>: the model would simulate
        /// a dwelling ventilated below the rate the Approved Document requires of it. A design duty
        /// <i>above</i> the requirement is design headroom - the mechanism an Approved Document O
        /// iteration raises a failing room through - and is <b>noted</b> rather than refused. Iteration 1a
        /// compared the two with an absolute difference, which was right while the design was defined as
        /// realizing the requirement exactly and which would now refuse every optimised dwelling; see
        /// <see cref="Refuse"/>.
        /// </para>
        /// <para>
        /// <b>And the floor is checked per ROOM as well as at the total.</b> Approved Document F requires
        /// a rate of each room, not an average over the dwelling, so a room below its own requirement
        /// refuses even where another room's surplus makes the system total agree exactly. Surplus is not
        /// tradeable between rooms: an earlier revision only warned here, and passed a model that
        /// simulated a bedroom ventilated below its Approved Document rate while reporting compliance.
        /// Two terminals of 10 l/s realizing a 20 l/s requirement agree; two of 15 are headroom and are
        /// reported; two of 8 are a shortfall and refuse.
        /// </para>
        /// <para>
        /// This is the <b>terminal/space</b> half of the Iteration 2 invariant. The capacity ceiling is
        /// the other half and is asked at the air handling unit's duty, where the equipment is - see
        /// <c>Query.IsVentilationUnitSufficient</c>. Neither check substitutes for the other.
        /// </para>
        /// </summary>
        /// <param name="tolerance_Lps">
        /// The margin in l/s within which two rates count as agreeing. A flow-rate literal rather than a
        /// borrowed distance tolerance - see <see cref="PartFSystemCapabilityRequirement"/> for the same
        /// reasoning.
        /// </param>
        /// <returns>Whether the design duty meets the requirement on both sides.</returns>
        public static bool ReconcileVentilationSystemDesignDuty(this AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, out List<string> notes, out List<string> warnings, out List<string> refusals, double tolerance_Lps = 0.001)
        {
            notes = [];
            warnings = [];
            refusals = [];

            //Checked before every comparison below - see Query.IsValidFlowRateTolerance.
            if (!IsValidFlowRateTolerance(tolerance_Lps))
            {
                refusals.Add(FlowRateToleranceRefusal(tolerance_Lps));

                return false;
            }

            if (adjacencyCluster is null || ventilationSystem is null)
            {
                refusals.Add("No ventilation system was supplied, so its design duty could not be checked against the Approved Document F requirement.");

                return false;
            }

            adjacencyCluster.VentilationSystemDesignDuty(ventilationSystem, out double supplyDuty_Lps, out double extractDuty_Lps);

            double requirement_Supply_Lps = 0;
            double requirement_Extract_Lps = 0;

            List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? [];

            foreach (Space space in spaces)
            {
                PartFSpaceData partFSpaceData = space?.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData is null)
                {
                    continue;
                }

                double space_Requirement_Supply_Lps = partFSpaceData.ContinuousSupplyFlowRate_Lps ?? 0;
                double space_Requirement_Extract_Lps = partFSpaceData.ContinuousExtractFlowRate_Lps ?? 0;

                requirement_Supply_Lps += space_Requirement_Supply_Lps;
                requirement_Extract_Lps += space_Requirement_Extract_Lps;

                //THIS system's terminals in that room, not the room's whole population.
                //
                //A room can hold terminals belonging to more than one ventilation system. Summing all of
                //them and calling the total this system's duty would let ANOTHER system's air satisfy this
                //system's Approved Document F floor - the room check below would pass while this system's
                //own terminal in the room stayed short, and with a little headroom in a neighbouring room
                //the system total would pass too. Both halves of the compliance check would then rest on
                //air this system does not move.
                //
                //Filtered rather than refused, and deliberately: this is a READ, and the honest answer to
                //"what does this system put into this room" is available exactly. Writes stay conservative
                //- Modify.ApplyTargetedDesignAirFlow still refuses outright to touch a room whose terminals
                //it cannot attribute unambiguously, because a write cannot be filtered the same way.
                //
                //For the single-system dwelling the Approved Document O workflow builds, this is the same
                //set as before and nothing changes.
                List<VentilationTerminal> ventilationTerminals_Space = [];
                foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.VentilationTerminals(space) ?? [])
                {
                    if ((adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal) ?? []).Find(x => x is not null && x.Guid == ventilationSystem.Guid) is not null)
                    {
                        ventilationTerminals_Space.Add(ventilationTerminal);
                    }
                }

                double space_Duty_Supply_Lps = ventilationTerminals_Space.VentilationTerminalDesignDuty_Lps(FlowClassification.Supply) ?? 0;
                double space_Duty_Extract_Lps = ventilationTerminals_Space.VentilationTerminalDesignDuty_Lps(FlowClassification.Extract) ?? 0;

                Note(notes, warnings, space, "supply", space_Duty_Supply_Lps, space_Requirement_Supply_Lps, tolerance_Lps);
                Note(notes, warnings, space, "extract", space_Duty_Extract_Lps, space_Requirement_Extract_Lps, tolerance_Lps);

                //THE FLOOR IS PER ROOM, and checking it only at the system total is not the same check.
                //
                //Approved Document F requires a rate of each room, not an average over the dwelling. A
                //bedroom 5 l/s under its requirement and a living room 5 l/s over it sum to a system total
                //that agrees exactly, and an earlier revision passed that model - simulating a bedroom
                //ventilated below the rate the Approved Document requires of it while reporting compliance.
                //Surplus in one room does not discharge another room's requirement and cannot be traded
                //against it.
                //
                //Above the requirement stays legal here, exactly as it is at the system total: it is design
                //headroom, it is what an Approved Document O iteration spends, and Note() has already
                //reported it.
                RefuseSpace(refusals, space, "supply", space_Duty_Supply_Lps, space_Requirement_Supply_Lps, tolerance_Lps);
                RefuseSpace(refusals, space, "extract", space_Duty_Extract_Lps, space_Requirement_Extract_Lps, tolerance_Lps);
            }

            bool result = refusals.Count == 0;

            result &= Refuse(refusals, ventilationSystem, "supply", supplyDuty_Lps, requirement_Supply_Lps, tolerance_Lps);
            result &= Refuse(refusals, ventilationSystem, "extract", extractDuty_Lps, requirement_Extract_Lps, tolerance_Lps);

            if (result)
            {
                notes.Add(string.Format(
                    "Ventilation system '{0}' design duty: supply {1:0.###} l/s, extract {2:0.###} l/s, summed from {3} design terminal(s) and meeting the Approved Document F requirement its spaces carry.",
                    ventilationSystem.FullName,
                    supplyDuty_Lps,
                    extractDuty_Lps,
                    (adjacencyCluster.VentilationTerminals(ventilationSystem) ?? []).Count));

                //A design above the requirement is legal and deliberate, but it is never silent: the whole
                //reason the two totals are derived separately is so that the gap between them can be read.
                if (supplyDuty_Lps > requirement_Supply_Lps + tolerance_Lps || extractDuty_Lps > requirement_Extract_Lps + tolerance_Lps)
                {
                    notes.Add(string.Format(
                        "Ventilation system '{0}' is designed above the Approved Document F requirement of {1:0.###} l/s supply and {2:0.###} l/s extract, by {3:0.###} l/s supply and {4:0.###} l/s extract. That difference is a design decision, not a recalculated requirement - the requirement is unchanged.",
                        ventilationSystem.FullName,
                        requirement_Supply_Lps,
                        requirement_Extract_Lps,
                        supplyDuty_Lps - requirement_Supply_Lps,
                        extractDuty_Lps - requirement_Extract_Lps));
                }
            }

            return result;
        }

        private static void Note(List<string> notes, List<string> warnings, Space space, string direction, double duty_Lps, double requirement_Lps, double tolerance_Lps)
        {
            if (System.Math.Abs(duty_Lps - requirement_Lps) <= tolerance_Lps)
            {
                return;
            }

            string note = duty_Lps > requirement_Lps
                ? string.Format(
                    "Space '{0}': the design {1} terminals total {2:0.###} l/s against the {3:0.###} l/s Approved Document F sized, so {4:0.###} l/s of that room's airflow is design headroom above the requirement. The design terminals are what will be simulated; this is reported so the difference is a decision rather than a surprise.",
                    space.Name,
                    direction,
                    duty_Lps,
                    requirement_Lps,
                    duty_Lps - requirement_Lps)
                : string.Format(
                    "Space '{0}': the design {1} terminals total {2:0.###} l/s but Approved Document F sized {3:0.###} l/s, so that room is designed below its regulatory minimum.",
                    space.Name,
                    direction,
                    duty_Lps,
                    requirement_Lps);

            notes.Add(note);
            warnings.Add(note);
        }

        /// <summary>
        /// Refuses a design duty <b>below</b> the Approved Document F requirement, and accepts one above
        /// it as design headroom.
        /// <para>
        /// <b>The asymmetry is the point, and it is an Iteration 2 correction.</b> Iteration 1a compared
        /// the two totals with an absolute difference, which was right while the design was defined as
        /// realizing the requirement exactly - but it hard-codes <c>Design == Required</c> and makes the
        /// invariant this iteration is built on, <c>PartFRequired &lt;= Design &lt;= SelectedCapacity</c>,
        /// impossible to express. An Approved Document O iteration that raises one failing bedroom from
        /// 20 to 24 l/s would have been refused as a model nobody sized.
        /// </para>
        /// <para>
        /// Below the requirement is still a <b>refusal</b>, and for the original reason: the model would
        /// otherwise simulate a dwelling ventilated below the rate the Approved Document requires of it,
        /// and no design intent makes that a legal building. Above the requirement is a deliberate design
        /// choice - the whole mechanism Part O optimisation works through - so it is <b>noted</b>, room by
        /// room and at the system total, and never silently accepted either.
        /// </para>
        /// </summary>
        /// <summary>
        /// Refuses one room designed below its own Approved Document F requirement.
        /// <para>
        /// The room-level half of the Iteration 2 invariant. The floor is enforced here, at the
        /// terminal/space level where the Approved Document states it; the capacity ceiling is a different
        /// question asked at the air handling unit's duty, where the equipment actually is. Neither
        /// substitutes for the other, and a system total is not a substitute for either.
        /// </para>
        /// </summary>
        private static void RefuseSpace(List<string> refusals, Space space, string direction, double duty_Lps, double requirement_Lps, double tolerance_Lps)
        {
            if (duty_Lps + tolerance_Lps >= requirement_Lps)
            {
                return;
            }

            refusals.Add(string.Format(
                "Space '{0}' has a design {1} airflow of {2:0.###} l/s, below the {3:0.###} l/s Approved Document F requires of that room. A shortfall in one room is not discharged by surplus in another, so nothing was prepared. Raise that room's design terminals to at least its requirement, or re-run the Part F calculation if the requirement itself is wrong.",
                space.Name,
                direction,
                duty_Lps,
                requirement_Lps));
        }

        private static bool Refuse(List<string> refusals, VentilationSystem ventilationSystem, string direction, double duty_Lps, double requirement_Lps, double tolerance_Lps)
        {
            if (duty_Lps + tolerance_Lps >= requirement_Lps)
            {
                return true;
            }

            refusals.Add(string.Format(
                "Ventilation system '{0}' has a design {1} duty of {2:0.###} l/s, below the {3:0.###} l/s the Approved Document F requirement on the spaces it serves totals. Design airflow is chosen above the regulatory minimum, never below it, so nothing was prepared. Re-run the Part F calculation, or raise the design terminal duties, so that the design meets the requirement.",
                ventilationSystem.FullName,
                direction,
                duty_Lps,
                requirement_Lps));

            return false;
        }
    }
}
