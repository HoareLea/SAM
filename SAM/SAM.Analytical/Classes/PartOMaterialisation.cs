// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// The outcome of <c>Modify.MaterialisePartODwellingStrategies</c>: either a materialised mixed model with
    /// the scenarios it is assessed under and the record of what it was built from, or structured refusals and
    /// <b>no model</b>. There is no partial success - a mixed model some of whose dwellings could not be built
    /// is not handed out.
    /// </summary>
    public class PartOMaterialisation
    {
        /// <summary>The materialised mixed model, or null where anything refused. Never the baseline itself.</summary>
        public AnalyticalModel AnalyticalModel { get; internal set; }

        /// <summary>Every reason nothing was materialised. Empty on success.</summary>
        public List<PartOMaterialisationRefusal> Refusals { get; } = [];

        /// <summary>Whether a model was materialised.</summary>
        public bool IsMaterialised => AnalyticalModel is not null && Refusals.Count == 0;

        /// <summary>
        /// One scenario per assessed zone: each dwelling at its own route's base iteration, each assessed
        /// communal corridor at the iteration-neutral common-space identity.
        /// </summary>
        public List<OverheatingScenario> OverheatingScenarios { get; } = [];

        /// <summary>What the model was built from; also stamped on the model.</summary>
        public PartOMaterialisationRecord Record { get; internal set; }

        /// <summary>The MVHR systems materialised, one per MVHR dwelling, in dwelling order.</summary>
        public List<VentilationSystem> VentilationSystems { get; } = [];

        /// <summary>The units those systems are supplied from, in the same order.</summary>
        public List<AirHandlingUnit> AirHandlingUnits { get; } = [];

        /// <summary>Every product selected, automatically or by explicit reference.</summary>
        public List<VentilationUnitSelection> VentilationUnitSelections { get; } = [];

        /// <summary>What was done, one sentence each.</summary>
        public List<string> Notes { get; } = [];

        /// <summary>What an engineer should look at although nothing refused.</summary>
        public List<string> Warnings { get; } = [];

        /// <summary>Every refusal message, for a caller that only shows text.</summary>
        public string Refusal => Refusals.Count == 0 ? null : string.Join(" ", Refusals.ConvertAll(x => x.Message));
    }
}
