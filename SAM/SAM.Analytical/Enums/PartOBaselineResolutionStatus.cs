// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>The outcome of asking where a <see cref="PartOModelReference"/> is now. See <c>Query.PartOModelResolution</c>.</summary>
    [Description("Part O Baseline Resolution Status.")]
    public enum PartOBaselineResolutionStatus
    {
        /// <summary>The reference is absent, unreadable or of a schema this build does not know. Nothing is inferred.</summary>
        [Description("Unknown")] Unknown,

        /// <summary>The model was found, is the same model by identity, and is in the state the result was derived from.</summary>
        [Description("Resolved")] Resolved,

        /// <summary>The model was found by identity but it has changed since the result was derived from it.</summary>
        [Description("Changed")] Changed,

        /// <summary>No file at any locator is this model.</summary>
        [Description("Not found")] NotFound,

        /// <summary>More than one file beside the recorded location is this model, so none is chosen.</summary>
        [Description("Ambiguous")] Ambiguous,
    }
}
