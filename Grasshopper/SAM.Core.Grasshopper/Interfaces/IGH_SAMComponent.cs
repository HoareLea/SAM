// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core.Grasshopper
{
    public interface IGH_SAMComponent
    {
        string ComponentVersion { get; }

        string SAMVersion { get; }

        string LatestComponentVersion { get; }
    }
}
