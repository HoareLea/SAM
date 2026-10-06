// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What a selected-equipment <b>capacity envelope</b> came to for one serving equipment group - the
    /// diagnostic answer to "what design could this dwelling's already-selected unit support?" - or why no
    /// envelope was calculated for it.
    /// <para>
    /// <b>Every "no" is a stated one.</b> An envelope is a diagnostic, and a diagnostic that silently
    /// produces nothing is worse than no diagnostic at all: an engineer reading a run that stopped with
    /// rooms still failing has to be able to tell "the selected unit has nothing left to give" from "the
    /// envelope was never worked out for this group". So each value below is a different fact about the
    /// design, and <see cref="Modify.EvaluateDesignAirFlowCapacityEnvelope"/> records exactly one of them
    /// per group.
    /// </para>
    /// <para>
    /// <b>None of these is an optimisation outcome.</b> An envelope never becomes the accepted design and
    /// is never another round - see <see cref="DesignAirFlowCapacityEnvelope"/>.
    /// </para>
    /// </summary>
    public enum DesignAirFlowCapacityEnvelopeOutcome
    {
        /// <summary>The envelope has not been worked out. Never a final state.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// The design the group's equipment already serves was grown proportionally - every terminal keeping
        /// its share of it - to the point where the first selected-equipment capacity constraint binds, and
        /// the resulting design is valid.
        /// <para>
        /// <b>The factor is bounded by feasibility, not by one step.</b> Where the ordinary optimisation
        /// stopped on its iteration guard with capacity still to spare, it is well above 1. It is never
        /// below 1: a design already on - or past - its rating is <see cref="NoHeadroom"/> instead, because
        /// an envelope never designs a dwelling downwards in the name of a diagnostic.
        /// </para>
        /// </summary>
        [Description("Scaled")] Scaled,

        /// <summary>
        /// The selected unit is already at - or past - its rating, or moves no design air at all, so there
        /// is no useful headroom for its design to be grown into. The ordinary optimisation's last accepted
        /// design <b>is</b> the envelope for this group, and saying so is the diagnostic.
        /// </summary>
        [Description("No Headroom")] NoHeadroom,

        /// <summary>
        /// No eligible deliberate target could be formed at all, so no failing room says which equipment
        /// the diagnostic is about. Not a failure - a design with nothing left to target has nothing to
        /// diagnose.
        /// </summary>
        [Description("No Targets")] NoTargets,

        /// <summary>
        /// The equipment or its capacity could not be resolved: no air handling unit resolves for the
        /// group's systems, nothing is selected on it, the selection is not among the products offered, or
        /// the design duty the rating would be compared against cannot be derived.
        /// <para>
        /// <b>An unknown ceiling is never an unlimited one.</b> Growing a design towards a capacity nobody
        /// can state would produce a design airflow with no authority behind it, which is precisely what the
        /// <c>PartFRequiredAirFlow != DesignAirFlow != SelectedEquipmentCapacity</c> separation exists to
        /// prevent.
        /// </para>
        /// </summary>
        [Description("Capacity Unresolved")] CapacityUnresolved,

        /// <summary>
        /// The design vector was read, a feasible factor found, and no growth of that design produces a
        /// valid one - because the design itself is not valid to grow: an unbalanced dwelling, a room
        /// already below its Approved Document F floor, terminals that cannot be attributed. The reason is
        /// recorded; nothing is repaired to make an envelope possible.
        /// </summary>
        [Description("Refused")] Refused,
    }
}
