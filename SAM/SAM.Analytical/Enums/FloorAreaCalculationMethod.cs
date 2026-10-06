// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Reports which step of the canonical space floor area hierarchy produced a value.
    /// See <see cref="Query.FloorArea(AdjacencyCluster, Space, out FloorAreaCalculationMethod, double, double, double, double)"/>.
    /// </summary>
    [Description("Space Floor Area Calculation Method.")]
    public enum FloorAreaCalculationMethod
    {
        /// <summary>No usable floor area could be established and none was already stored.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>Sum of the actual surface areas of the space's geometrical floor panels. The canonical result: it follows a ramped or tilted floor rather than its horizontal projection.</summary>
        [Description("Geometrical Floor Panels")] GeometricalFloorPanels,

        /// <summary>Horizontal section through the middle of the space shell. Fallback only, used when no reliable geometrical floor panel is available; for a ramp this is a plan area, not the walking surface.</summary>
        [Description("Horizontal Section")] HorizontalSection,

        /// <summary>Neither calculation produced a valid value, so the space's already-stored area was kept.</summary>
        [Description("Existing")] Existing
    }
}
