// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Condensed fabric buckets for the space fabric table. Opaque rows hold net panel area (apertures removed);
    /// window and door rows hold pane and frame areas.
    /// </summary>
    public enum FabricCategory
    {
        [Description("Walls")] Walls,
        [Description("Windows")] Windows,
        [Description("Doors")] Doors,
        [Description("Roofs / ceilings")] RoofsAndCeilings,
        [Description("Ground floors")] GroundFloors,
        [Description("Other floors")] OtherFloors,
    }
}
