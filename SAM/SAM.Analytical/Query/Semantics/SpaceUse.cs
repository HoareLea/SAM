// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Converts a CIBSE TM59 classification to the shared <see cref="Analytical.SpaceUse"/>.
        /// <para>
        /// TM59 deliberately groups every non-habitable space - bathroom, communal corridor, stairs,
        /// cupboard, riser - into a single NonHabitable classification, because none of them carries a
        /// prescribed overheating criterion. That distinction matters to Approved Document F, which
        /// needs different extract rates for a bathroom (8 l/s), sanitary accommodation (6 l/s) and a
        /// utility room (8 l/s), and no terminal at all for circulation. NonHabitable therefore maps to
        /// Undefined rather than being forced into a guess: the caller must resolve it from the space
        /// name or an explicit override.
        /// </para>
        /// </summary>
        public static SpaceUse SpaceUse(this TM59SpaceClassification tM59SpaceClassification)
        {
            switch (tM59SpaceClassification)
            {
                case Analytical.TM59SpaceClassification.Bedroom:
                    return Analytical.SpaceUse.Bedroom;

                case Analytical.TM59SpaceClassification.LivingRoom:
                    return Analytical.SpaceUse.LivingRoom;

                case Analytical.TM59SpaceClassification.Kitchen:
                    return Analytical.SpaceUse.Kitchen;

                case Analytical.TM59SpaceClassification.LivingRoomKitchen:
                    return Analytical.SpaceUse.LivingRoomKitchen;

                case Analytical.TM59SpaceClassification.Studio:
                    return Analytical.SpaceUse.Studio;

                default:
                    return Analytical.SpaceUse.Undefined;
            }
        }

        /// <summary>
        /// Converts the shared <see cref="Analytical.SpaceUse"/> to a CIBSE TM59 classification. Every
        /// space use that TM59 regards as non-habitable collapses to NonHabitable, so this direction is
        /// lossy by design.
        /// </summary>
        public static TM59SpaceClassification TM59SpaceClassification(this SpaceUse spaceUse)
        {
            switch (spaceUse)
            {
                case Analytical.SpaceUse.Bedroom:
                    return Analytical.TM59SpaceClassification.Bedroom;

                case Analytical.SpaceUse.LivingRoom:
                    return Analytical.TM59SpaceClassification.LivingRoom;

                case Analytical.SpaceUse.Kitchen:
                    return Analytical.TM59SpaceClassification.Kitchen;

                case Analytical.SpaceUse.LivingRoomKitchen:
                    return Analytical.TM59SpaceClassification.LivingRoomKitchen;

                case Analytical.SpaceUse.Studio:
                    return Analytical.TM59SpaceClassification.Studio;

                case Analytical.SpaceUse.Undefined:
                    return Analytical.TM59SpaceClassification.Undefined;

                default:
                    return Analytical.TM59SpaceClassification.NonHabitable;
            }
        }
    }
}
