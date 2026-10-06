// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>What a <see cref="PartOModelReference"/> points at.</summary>
    [Description("Part O Model Reference Kind.")]
    public enum PartOModelReferenceKind
    {
        /// <summary>Not stated, or unreadable.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>The engineer's design model: design intent and Part O inputs, never Part O outputs.</summary>
        [Description("Design")] Design,

        /// <summary>A saved Part O result model (its run's own <c>.sam</c>).</summary>
        [Description("Result")] Result,
    }
}
