// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    /// <summary>
    /// A room an optimisation round was asked to target and <b>cannot</b>, with the reason - the explicit
    /// answer to "not automatically optimisable", stated rather than left as a silent omission.
    /// <para>
    /// <b>Why this is not a refusal of the round.</b> A room with no Approved Document O design terminal on
    /// the side being asked for is not a broken dwelling; it is a room this optimisation has no lever on.
    /// Inventing a terminal would size a duty the Approved Document F assessment never asked for, and
    /// refusing the whole round would stop every other failing room in the building because of one. So the
    /// target is dropped, named, and the round goes on with the rest - and a caller that finds every target
    /// here knows it has nothing left to try.
    /// </para>
    /// <para>
    /// <b>Only a coherent request lands here.</b> A target that is not a design airflow at all - no room, a
    /// direction that is neither supply nor extract, a rate that is not a finite non-negative number, or a
    /// room that is not in the model - refuses the whole round instead. Dropping one of those and applying
    /// the rest would execute part of a transaction the caller asked for as a whole.
    /// </para>
    /// </summary>
    public class DesignAirFlowTargetRefusal
    {
        internal DesignAirFlowTargetRefusal(DesignAirFlowTarget designAirFlowTarget, string reason)
        {
            DesignAirFlowTarget = designAirFlowTarget;
            Reason = reason;
        }

        /// <summary>The target that was asked for.</summary>
        public DesignAirFlowTarget DesignAirFlowTarget { get; }

        /// <summary>Why it could not be taken, in one sentence.</summary>
        public string Reason { get; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", DesignAirFlowTarget, Reason);
        }
    }
}
