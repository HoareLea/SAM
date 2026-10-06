// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// How a materialised mixed Part O model is simulated in TAS - decided by SAM from the selected strategies,
    /// never by the caller.
    /// <para>
    /// <b>One route for the whole model, never a hybrid.</b> <see cref="Izam"/> when no dwelling is cooled: the
    /// TBD with each unit's plant zone and IZAMs. <see cref="Systems"/> as soon as any dwelling is cooled: the no-IZAM
    /// TBD, TAS Systems (TPD) for every mechanical dwelling and the thermostat bridge, with natural dwellings and
    /// communal corridors free-running (<c>documentation/PartO-MixedDwellingStrategies-PR3A.md</c> §14.1).
    /// </para>
    /// </summary>
    [Description("Part O Simulation Route.")]
    public enum PartOSimulationRoute
    {
        /// <summary>Not stated, or unreadable. Never read as either route.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>No dwelling is cooled: the TBD/IZAM route.</summary>
        [Description("IZAM")] Izam,

        /// <summary>A dwelling is cooled: the TAS Systems (TPD) route for the whole model.</summary>
        [Description("Systems")] Systems,
    }
}
