// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using System;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Space identity. Phase 1 has no space number: the name is the identifier (decision D3).
    /// </summary>
    public sealed class SpaceIdentityData
    {
        public Guid Guid { get; init; }

        public ReportValue<string> Name { get; init; }

        public ReportValue<string> LevelName { get; init; }

        public ReportValue<string> InternalConditionName { get; init; }
    }
}
