// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// How air is allowed to move between two rooms of one dwelling.
    /// <para>
    /// Approved Document F, Volume 1: Dwellings (2021 edition, England) paragraph 1.25 (page 10)
    /// requires internal doors to allow air to flow through the dwelling by providing a minimum free
    /// area equivalent to a 10mm undercut in a 760mm wide door. A door undercut is the guidance's own
    /// example; any permanent opening of at least the equivalent free area serves the same purpose.
    /// </para>
    /// </summary>
    public enum PartFTransferDeviceType
    {
        /// <summary>
        /// No transfer provision is represented. An ordinary closed doorway with no undercut, grille or
        /// permanent opening is not a transfer path.
        /// </summary>
        [Description("Not Represented")] NotRepresented,

        /// <summary>A door undercut, the arrangement described by paragraph 1.25.</summary>
        [Description("Door Undercut")] DoorUndercut,

        /// <summary>A transfer grille in the door or the partition.</summary>
        [Description("Transfer Grille")] TransferGrille,

        /// <summary>
        /// A permanent opening between rooms with no means of closing it (Appendix A, page 37).
        /// </summary>
        [Description("Permanent Opening")] PermanentOpening,

        /// <summary>An open passage or doorway with no door leaf at all.</summary>
        [Description("Open Passage")] OpenPassage,

        /// <summary>Another explicitly represented permanent transfer provision.</summary>
        [Description("Other")] Other,
    }
}
