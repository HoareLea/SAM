// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Create
    {
        /// <summary>
        /// Builds the shared semantic classification for a <see cref="Analytical.SpaceUse"/>. This is
        /// the single convention table that derives every independent semantic role from the primary
        /// use, so Approved Document F, Approved Document O, CIBSE TM59 and SAM_UI all read the same
        /// answer for the same space.
        /// <para>
        /// The habitable and wet room columns follow Approved Document F, Volume 1: Dwellings (2021
        /// edition, for use in England), Appendix A. A habitable room is one used for dwelling
        /// purposes but not <i>solely</i> a kitchen, utility room, bathroom, cellar or sanitary
        /// accommodation - so an open plan living kitchen is habitable while a room that is solely a
        /// kitchen is not. A wet room is a room used for domestic activities producing significant
        /// airborne moisture, with sanitary accommodation also regarded as a wet room for Part F.
        /// </para>
        /// <para>
        /// The supply and extract role columns are the SAM design convention, not a direct quotation
        /// of the Approved Document: a studio and an open plan living kitchen are given the supply
        /// role only, even though each also contains the cooking function that paragraph 1.17a and
        /// Table 1.2 require extract from. SAM does not model a space with both a supply and an
        /// extract terminal. The cooking function is carried by IsCookingSpace so the Part F
        /// calculation can warn about the missing kitchen extract rather than silently drop it.
        /// </para>
        /// </summary>
        public static SpaceSemantics SpaceSemantics(this SpaceUse spaceUse, SpaceSemanticsSource source = SpaceSemanticsSource.None, string matchedAlias = null, string diagnostic = null)
        {
            //                             dwelling habitable bedroomEq living cooking wetRoom circulation communal supply extract
            switch (spaceUse)
            {
                case Analytical.SpaceUse.Bedroom:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, true, true, false, false, false, false, false, true, false);

                case Analytical.SpaceUse.LivingRoom:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, true, false, true, false, false, false, false, true, false);

                //Solely a kitchen, so not a habitable room. A wet room taking the extract terminal.
                case Analytical.SpaceUse.Kitchen:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, false, false, false, true, true, false, false, false, true);

                //Not solely a kitchen, so habitable and given the supply terminal. Still a cooking
                //space, so the kitchen extract requirement is reported rather than assigned.
                case Analytical.SpaceUse.LivingRoomKitchen:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, true, false, true, true, false, false, false, true, false);

                //Sleeping, living and cooking in one room. Counted as one bedroom.
                case Analytical.SpaceUse.Studio:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, true, true, true, true, false, false, false, true, false);

                case Analytical.SpaceUse.Bathroom:
                case Analytical.SpaceUse.Ensuite:
                case Analytical.SpaceUse.UtilityRoom:
                case Analytical.SpaceUse.SanitaryAccommodation:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, false, false, false, false, true, false, false, false, true);

                case Analytical.SpaceUse.Circulation:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, false, false, false, false, false, true, false, false, false);

                //Shared between dwellings, so outside any one dwelling and given no terminal by the
                //dwelling calculation. Sized to Approved Document F, Volume 2 instead.
                case Analytical.SpaceUse.CommunalCirculation:
                    return New(spaceUse, source, matchedAlias, diagnostic, false, false, false, false, false, false, true, true, false, false);

                case Analytical.SpaceUse.Storage:
                case Analytical.SpaceUse.PlantRoom:
                case Analytical.SpaceUse.Void:
                    return New(spaceUse, source, matchedAlias, diagnostic, true, false, false, false, false, false, false, false, false, false);

                case Analytical.SpaceUse.NonDwelling:
                    return New(spaceUse, source, matchedAlias, diagnostic, false, false, false, false, false, false, false, false, false, false);

                default:
                    //Undefined. Deliberately not assumed to be outside the dwelling: an unrecognised
                    //name is a reporting problem, not evidence that the space is communal.
                    return New(Analytical.SpaceUse.Undefined, source, matchedAlias, diagnostic, true, false, false, false, false, false, false, false, false, false);
            }
        }

        private static SpaceSemantics New(
            SpaceUse spaceUse,
            SpaceSemanticsSource source,
            string matchedAlias,
            string diagnostic,
            bool isDwellingSpace,
            bool isHabitable,
            bool isBedroomEquivalent,
            bool isLivingSpace,
            bool isCookingSpace,
            bool isWetRoom,
            bool isCirculation,
            bool isCommunal,
            bool hasSupplyRole,
            bool hasExtractRole)
        {
            return new SpaceSemantics(
                spaceUse,
                source,
                matchedAlias,
                diagnostic,
                isDwellingSpace,
                isHabitable,
                isBedroomEquivalent,
                isLivingSpace,
                isCookingSpace,
                isWetRoom,
                isCirculation,
                isCommunal,
                hasSupplyRole,
                hasExtractRole);
        }
    }
}
