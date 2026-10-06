// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical
{
    /// <summary>
    /// One room's design airflow moving from one figure to another, in one direction, as part of a single
    /// design transaction.
    /// <para>
    /// <b>Whether it was chosen or caused is on the record.</b> An Approved Document O iteration targets
    /// the room that failed and nothing else; the other rooms that move do so because a balanced system
    /// has to keep moving the air it takes in. Those are different engineering statements and collapsing
    /// them would make it impossible to say afterwards which rooms were design decisions - see
    /// <see cref="IsDerived"/>.
    /// </para>
    /// <para>
    /// A report of what happened, not a thing the model stores. The design airflows themselves live on
    /// the terminals, where they always have; nothing here is a fifth authority beside the four Iteration
    /// 2 keeps apart.
    /// </para>
    /// </summary>
    public class DesignAirFlowAdjustment
    {
        public DesignAirFlowAdjustment(Guid spaceGuid, string spaceName, FlowClassification flowClassification, double before_Lps, double after_Lps, double requirement_Lps, bool isDerived)
        {
            SpaceGuid = spaceGuid;
            SpaceName = spaceName;
            FlowClassification = flowClassification;
            Before_Lps = before_Lps;
            After_Lps = after_Lps;
            Requirement_Lps = requirement_Lps;
            IsDerived = isDerived;
        }

        /// <summary>The room.</summary>
        public Guid SpaceGuid { get; }

        /// <summary>The room's name, so a report reads without resolving every guid back through the model.</summary>
        public string SpaceName { get; }

        /// <summary>Which side of the system moved. The two are independent and one adjustment names one of them.</summary>
        public FlowClassification FlowClassification { get; }

        /// <summary>The room's design airflow [l/s] in that direction before the transaction.</summary>
        public double Before_Lps { get; }

        /// <summary>And after it.</summary>
        public double After_Lps { get; }

        /// <summary>
        /// What Approved Document F requires of that room in that direction, <see cref="double.NaN"/>
        /// where it requires nothing. Carried so a report can show that the design stayed above the floor
        /// without re-deriving it, and it is <b>never</b> altered by a transaction.
        /// </summary>
        public double Requirement_Lps { get; }

        /// <summary>
        /// <b>False for the room the change was aimed at; true for a room that moved as a consequence.</b>
        /// <para>
        /// This is the distinction the whole class exists for. Raising a failing bedroom from 20 to 24 l/s
        /// is an explicit optimisation target. The wet room whose extract rises by the matching 4 l/s was
        /// not selected as a target and must never be reported as though it were: it moved because the
        /// dwelling's balanced network has to carry the extra air, decided by the extract allocation
        /// strategy the Part F calculation already names.
        /// </para>
        /// </summary>
        public bool IsDerived { get; }

        /// <summary>How much the room moved [l/s]. Negative where it came down.</summary>
        public double Change_Lps
        {
            get
            {
                return After_Lps - Before_Lps;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0} {1}: {2:0.###} -> {3:0.###} l/s ({4})",
                SpaceName,
                Core.Query.Description(FlowClassification),
                Before_Lps,
                After_Lps,
                IsDerived ? "derived" : "targeted");
        }
    }
}
