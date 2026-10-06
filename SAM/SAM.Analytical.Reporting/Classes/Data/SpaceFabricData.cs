// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;
using System.Collections.Generic;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Areas of one fabric bucket, split by exposure. An element is external when it bounds only this space.
    /// </summary>
    public sealed class FabricAreaRow
    {
        public FabricCategory Category { get; init; }

        /// <summary>
        /// External area [m²]: net opaque area, or the pane area of windows / doors.
        /// </summary>
        public ReportValue<Quantity> ExternalArea { get; init; }

        public ReportValue<Quantity> InternalArea { get; init; }

        /// <summary>
        /// External frame area [m²] of windows / doors; not applicable to opaque rows.
        /// </summary>
        public ReportValue<Quantity> ExternalFrameArea { get; init; }

        public ReportValue<Quantity> InternalFrameArea { get; init; }
    }

    /// <summary>
    /// Fabric areas bounding the space, from SAM geometry (no TBD needed).
    /// </summary>
    public sealed class SpaceFabricData
    {
        /// <summary>
        /// One row per <see cref="FabricCategory"/>, in enum order. Empty when no panels bound the space.
        /// </summary>
        public IReadOnlyList<FabricAreaRow> Rows { get; init; }
    }
}
