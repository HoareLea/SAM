// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical
{
    /// <summary>
    /// One <b>deliberate</b> design airflow the caller is asking for: this room, this side, this figure.
    /// <para>
    /// <b>An input, and only an input.</b> It states intent and carries no verdict - what a round actually
    /// achieved is <see cref="DesignAirFlowAdjustment"/>, which the round produces and which keeps the
    /// targeted/derived distinction. Nothing here is stored in the model.
    /// </para>
    /// <para>
    /// <b>A target is never a derived room.</b> The balancing consequence of a round is worked out over
    /// the rooms nobody targeted - see <see cref="Modify.EvaluateTargetedDesignAirFlows"/> - so putting a
    /// room in this list is the statement "this room was chosen", with everything that implies for how a
    /// report has to describe it afterwards.
    /// </para>
    /// </summary>
    public class DesignAirFlowTarget
    {
        /// <param name="space">The room being targeted. Resolved by <see cref="Core.SAMObject.Guid"/> against
        /// the model the round is evaluated on, never trusted as handed in.</param>
        /// <param name="flowClassification">Which side of that room is being asked to move.</param>
        /// <param name="designFlowRate_Lps">The design airflow being asked for [l/s], as a total across the
        /// room's terminals of that direction.</param>
        public DesignAirFlowTarget(Space space, FlowClassification flowClassification, double designFlowRate_Lps)
        {
            Space = space;
            FlowClassification = flowClassification;
            DesignFlowRate_Lps = designFlowRate_Lps;
        }

        /// <summary>The room this target names.</summary>
        public Space Space { get; }

        /// <summary>Its guid, or <see cref="Guid.Empty"/> where no room was supplied.</summary>
        public Guid SpaceGuid
        {
            get
            {
                return Space is null ? Guid.Empty : Space.Guid;
            }
        }

        /// <summary>The room's name, so a refusal reads without resolving the guid back through the model.</summary>
        public string SpaceName
        {
            get
            {
                return Space?.Name;
            }
        }

        /// <summary>Which side of the room moves. Supply or extract; nothing else is a design airflow.</summary>
        public FlowClassification FlowClassification { get; }

        /// <summary>The design airflow being asked for [l/s].</summary>
        public double DesignFlowRate_Lps { get; }

        public override string ToString()
        {
            return string.Format(
                "{0} {1} -> {2:0.###} l/s",
                SpaceName ?? "-",
                Core.Query.Description(FlowClassification),
                DesignFlowRate_Lps);
        }
    }
}
