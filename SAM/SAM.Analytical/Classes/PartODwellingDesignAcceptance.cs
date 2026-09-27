// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// The outcome of <c>Modify.AcceptPartODwellingDesign</c>: either the baseline carrying one dwelling's accepted
    /// design airflow on its own design terminals, with the fingerprint a retained-design strategy records, or
    /// refusals and <b>no model</b>. There is no partial acceptance - acceptance is whole-dwelling (PR0 D3).
    /// </summary>
    public class PartODwellingDesignAcceptance
    {
        public PartODwellingDesignAcceptance(Guid guid_Zone)
        {
            ZoneGuid = guid_Zone;
        }

        /// <summary>The dwelling accepted.</summary>
        public Guid ZoneGuid { get; }

        /// <summary>
        /// The baseline with the dwelling's design terminals at the accepted airflow, or null where anything refused.
        /// A new model: the baseline handed in is never modified.
        /// </summary>
        public AnalyticalModel AnalyticalModel { get; internal set; }

        /// <summary>
        /// <c>Query.PartODwellingDesignFingerprint</c> of the dwelling on <see cref="AnalyticalModel"/> - what a
        /// <see cref="Enums.PartODesignAirFlowBasis.RetainedDesign"/> strategy records as its guard. Never an airflow.
        /// </summary>
        public string DesignFingerprint { get; internal set; }

        /// <summary>Each space and direction whose design airflow the acceptance changed, in space-name order.</summary>
        public List<PartODwellingDesignChange> Changes { get; } = [];

        /// <summary>What was realised or written, one sentence each.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>Every reason nothing was accepted. Empty on success.</summary>
        public List<string> Refusals { get; } = [];

        public bool IsAccepted => AnalyticalModel is not null && Refusals.Count == 0;

        public string Refusal => Refusals.Count == 0 ? null : string.Join(" ", Refusals);
    }

    /// <summary>One space's design airflow in one direction, before and after an accepted design was written.</summary>
    public class PartODwellingDesignChange
    {
        public PartODwellingDesignChange(Guid guid_Space, string name_Space, FlowClassification flowClassification, double before_Lps, double after_Lps)
        {
            SpaceGuid = guid_Space;
            SpaceName = name_Space;
            FlowClassification = flowClassification;
            Before_Lps = before_Lps;
            After_Lps = after_Lps;
        }

        public Guid SpaceGuid { get; }

        public string SpaceName { get; }

        public FlowClassification FlowClassification { get; }

        /// <summary>
        /// The baseline's design airflow before acceptance - the Approved Document F requirement where the dwelling
        /// carried no design terminals and they were realised for it.
        /// </summary>
        public double Before_Lps { get; }

        /// <summary>The accepted design airflow, now on the baseline's terminals.</summary>
        public double After_Lps { get; }
    }
}
