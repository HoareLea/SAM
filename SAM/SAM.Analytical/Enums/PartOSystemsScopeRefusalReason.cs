// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Why no Part O Systems materialisation scope could be taken - the structured half of a
    /// <c>PartOSystemsScopeRefusal</c>; its message is the other half.
    /// <para>Members are appended, never reordered.</para>
    /// </summary>
    [Description("Part O Systems Scope Refusal Reason.")]
    public enum PartOSystemsScopeRefusalReason
    {
        [Description("Undefined")] Undefined,

        /// <summary>No model (no adjacency cluster) was supplied.</summary>
        [Description("No Model")] NoModel,

        /// <summary>No identity of a ventilation system Part O built was supplied.</summary>
        [Description("No Identities")] NoIdentities,

        /// <summary>A supplied Part O system identity is not a ventilation system on the model.</summary>
        [Description("Identity Not On Model")] IdentityNotOnModel,

        /// <summary>An authored system carries effective mechanical duty through a terminal related to no space.</summary>
        [Description("Duty Serves No Space")] DutyServesNoSpace,

        /// <summary>An authored system carries effective mechanical duty in a room of the assessed dwellings.</summary>
        [Description("Duty Inside Dwelling Scope")] DutyInsideDwellingScope,

        /// <summary>An authored system carries effective mechanical duty in a room outside the assessed dwellings.</summary>
        [Description("Duty Outside Dwelling Scope")] DutyOutsideDwellingScope,

        /// <summary>A system the scope left out could not be removed from the working copy.</summary>
        [Description("Not Removable")] NotRemovable,
    }
}
