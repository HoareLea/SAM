// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// True where <paramref name="specific"/> is a more specific refinement of
        /// <paramref name="general"/> rather than a genuinely different, disagreeing classification -
        /// e.g. Communal Circulation is Circulation, just outside the dwelling boundary rather than
        /// inside it. Used so <see cref="SpaceSemanticsResolver"/> does not report a space
        /// name/internal-condition disagreement as a conflict when one source is simply more specific
        /// than the other.
        /// <para>
        /// Deliberately a short, explicit list rather than anything derived from the semantic role flags.
        /// Sanitary Accommodation is NOT a refinement of Bathroom even though both are wet rooms:
        /// Approved Document F Table 1.2 gives them different extract rates (8 l/s against 6 l/s), so
        /// treating one as the other would silently change a calculated rate. Only pairs that describe
        /// the same room at different specificity belong here.
        /// </para>
        /// </summary>
        public static bool IsCompatibleSpaceUseRefinement(Analytical.SpaceUse general, Analytical.SpaceUse specific)
        {
            //Communal circulation is circulation, outside the dwelling rather than inside it.
            if (general == Analytical.SpaceUse.Circulation && specific == Analytical.SpaceUse.CommunalCirculation)
            {
                return true;
            }

            //An ensuite is a bathroom accessed directly from a bedroom - the same room, said more
            //precisely. Both are wet rooms taking the same Table 1.2 extract rate.
            if (general == Analytical.SpaceUse.Bathroom && specific == Analytical.SpaceUse.Ensuite)
            {
                return true;
            }

            return false;
        }
    }
}
