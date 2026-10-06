// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// What a space is, independently of which standard is assessing it. This is the shared
    /// vocabulary of the semantic classification layer consumed by Approved Document F, Approved
    /// Document O, CIBSE TM59 and the SAM_UI internal condition mapping.
    /// <para>
    /// The shared layer describes what a space <i>is</i>; each standard decides how that space is
    /// <i>assessed</i>. Regulatory thresholds, terminal sizing and assessment criteria therefore do
    /// not belong here - see <see cref="SpaceSemantics"/> for the derived semantic flags, and
    /// <see cref="PartFCategory"/> for the Approved Document F rules keyed off this enum.
    /// </para>
    /// <para>
    /// This is a superset of <see cref="TM59SpaceClassification"/>. TM59 deliberately groups every
    /// non-habitable space into a single classification, so the mapping from this enum to TM59 is
    /// many to one and the reverse mapping is lossy - see Query.SpaceUse and
    /// Query.TM59SpaceClassification.
    /// </para>
    /// </summary>
    public enum SpaceUse
    {
        /// <summary>No defensible classification. Always reported, never guessed at.</summary>
        [Description("Undefined")] Undefined,

        /// <summary>A room for sleeping. Habitable, counts towards the bedroom count.</summary>
        [Description("Bedroom")] Bedroom,

        /// <summary>A living room, lounge, sitting or dining room with no cooking function.</summary>
        [Description("Living Room")] LivingRoom,

        /// <summary>
        /// A room that is solely a kitchen. Approved Document F, Volume 1 (2021 edition) Appendix A
        /// therefore makes it a wet room and not a habitable room.
        /// </summary>
        [Description("Kitchen")] Kitchen,

        /// <summary>
        /// An open plan room combining living and cooking. Approved Document F, Volume 1 (2021
        /// edition) Appendix A defines a habitable room as one that is not <i>solely</i> a kitchen,
        /// so this is both habitable and a cooking space.
        /// </summary>
        [Description("Living Room/Kitchen")] LivingRoomKitchen,

        /// <summary>
        /// A single room combining sleeping, living and cooking. Habitable, a cooking space, and
        /// counted as one bedroom.
        /// </summary>
        [Description("Studio")] Studio,

        /// <summary>
        /// A room containing a bath or shower, which may also contain sanitary accommodation
        /// (Approved Document F, Volume 1, 2021 edition, Appendix A). A wet room.
        /// </summary>
        [Description("Bathroom")] Bathroom,

        /// <summary>A bathroom accessed directly from a bedroom. A wet room.</summary>
        [Description("Ensuite")] Ensuite,

        /// <summary>A room used for clothes washing or similar domestic activity. A wet room.</summary>
        [Description("Utility Room")] UtilityRoom,

        /// <summary>
        /// A space containing one or more flush toilets or urinals (Approved Document F, Volume 1,
        /// 2021 edition, Appendix A). Regarded as a wet room for the purposes of Part F.
        /// </summary>
        [Description("Sanitary Accommodation")] SanitaryAccommodation,

        /// <summary>Circulation inside the dwelling: hall, landing, internal corridor, stair.</summary>
        [Description("Circulation")] Circulation,

        /// <summary>
        /// Circulation shared between dwellings: communal corridor, stairwell, lift or entrance
        /// lobby. Outside the dwelling, and assessed separately from it.
        /// </summary>
        [Description("Communal Circulation")] CommunalCirculation,

        /// <summary>A store, cupboard or closet.</summary>
        [Description("Storage")] Storage,

        /// <summary>A plant room, riser or similar space housing building services.</summary>
        [Description("Plant Room")] PlantRoom,

        /// <summary>A void or open to below area, excluded from the internal floor area.</summary>
        [Description("Void")] Void,

        /// <summary>
        /// A space that is positively identified as not forming part of any dwelling, for example a
        /// landlord area or a commercial unit in a mixed use building.
        /// </summary>
        [Description("Non Dwelling")] NonDwelling,
    }
}
