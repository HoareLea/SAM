// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    /// <summary>Where a <see cref="PartOModelReference"/> is now, and why that is the answer. See <c>Query.PartOModelResolution</c>.</summary>
    public class PartOBaselineResolution
    {
        public PartOBaselineResolution(PartOBaselineResolutionStatus status, string path, string description)
        {
            Status = status;
            Path = path;
            Description = description;
        }

        /// <summary>Found and unchanged, found and changed, not found, ambiguous, or unknown.</summary>
        public PartOBaselineResolutionStatus Status { get; }

        /// <summary>The file the model was found in: set for resolved and changed, otherwise null.</summary>
        public string Path { get; }

        /// <summary>One sentence saying what was found, for a person.</summary>
        public string Description { get; }

        /// <summary>Whether the model is where it was and as it was.</summary>
        public bool IsResolved => Status == PartOBaselineResolutionStatus.Resolved;

        public override string ToString() => Description;
    }
}
