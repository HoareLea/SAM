// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Which Part O case a saved result is, as stated by the run that produced it - persisted in its
    /// <see cref="PartOBaselineReference"/>, so the case no longer has to be inferred from an output folder, a file
    /// name suffix or a sidecar.
    /// </summary>
    [Description("Part O Derived Case.")]
    public enum PartODerivedCase
    {
        /// <summary>Not stated, or unreadable. Never read as any case.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>Iteration 1a, derived from the design model.</summary>
        [Description("Iteration 1a")] Iteration1a,

        /// <summary>Iteration 1b, derived from the design model.</summary>
        [Description("Iteration 1b")] Iteration1b,

        /// <summary>Iteration 2, derived from the design model.</summary>
        [Description("Iteration 2")] Iteration2,

        /// <summary>Iteration 2B, derived from the Iteration 2 result (its <see cref="PartOBaselineReference.Source"/>).</summary>
        [Description("Iteration 2B")] Iteration2B,

        /// <summary>Iteration 3, derived from the Iteration 1a or Iteration 2 result it is paired with.</summary>
        [Description("Iteration 3")] Iteration3,

        /// <summary>Mixed Design, derived from the design model directly.</summary>
        [Description("Mixed Design")] MixedDesign,
    }
}
