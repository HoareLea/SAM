// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// The opening type row of Approved Document F, Volume 1: Dwellings (2021 edition, England)
    /// Table 1.4 (page 11), which sets the minimum total area of purge openings as a fraction of the
    /// room's floor area.
    /// <para>
    /// The opening angle of a hinged or pivot window is a product property, not model geometry, so it
    /// is an explicit input and defaults to <see cref="Undefined"/> rather than being guessed at.
    /// </para>
    /// </summary>
    public enum PartFPurgeOpeningType
    {
        /// <summary>
        /// The opening type is not known, so the Table 1.4 fraction cannot be selected and the required
        /// opening area cannot be determined.
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Hinged or pivot window with an opening angle of 15 to 30 degrees: 1/10 of the floor area of
        /// the room.
        /// </summary>
        [Description("Hinged or Pivot Window 15 to 30 Degrees")] HingedOrPivot15To30Degrees,

        /// <summary>
        /// Hinged or pivot window with an opening angle of 30 degrees or more: 1/20 of the floor area of
        /// the room.
        /// </summary>
        [Description("Hinged or Pivot Window 30 Degrees or More")] HingedOrPivot30DegreesOrMore,

        /// <summary>Opening sash window: 1/20 of the floor area of the room.</summary>
        [Description("Opening Sash Window")] OpeningSashWindow,

        /// <summary>External door: 1/20 of the floor area of the room.</summary>
        [Description("External Door")] ExternalDoor,

        /// <summary>
        /// Hinged or pivot window with an opening angle of less than 15 degrees. Paragraph 1.31
        /// (page 11) states these are not suitable for purge ventilation, so such an opening contributes
        /// nothing.
        /// </summary>
        [Description("Hinged or Pivot Window Under 15 Degrees")] HingedOrPivotUnder15Degrees,
    }
}
