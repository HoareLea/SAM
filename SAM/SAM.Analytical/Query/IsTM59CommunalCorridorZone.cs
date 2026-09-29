// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Whether a common-space zone's spaces make it an assessed TM59 communal corridor - the one rule both the
        /// mixed-strategy materialisation and the homogeneous Part O preparation apply before stating a corridor's
        /// iteration-neutral scenario (<c>Create.PartOCommonSpaceOverheatingScenario</c>).
        /// <para>
        /// <b>true</b> where every space is assigned exactly the TM59 communal-corridor condition
        /// (<see cref="IsTM59CommunalCorridor(Space)"/> - never from a name); <b>false</b> where none is, or there
        /// are no spaces; <b>null</b> where the zone MIXES corridor and non-corridor spaces. An overheating scenario
        /// is zone-scoped and states one criterion for every space of its zone, so a mixed zone can be neither
        /// assessed as a corridor nor left out without being wrong - the caller says so rather than guessing.
        /// </para>
        /// </summary>
        public static bool? IsTM59CommunalCorridorZone(IEnumerable<Space> spaces)
        {
            int count = 0;
            int count_Corridor = 0;

            foreach (Space space in spaces ?? [])
            {
                if (space is null)
                {
                    continue;
                }

                count++;

                if (space.IsTM59CommunalCorridor())
                {
                    count_Corridor++;
                }
            }

            if (count_Corridor == 0)
            {
                return false;
            }

            return count_Corridor == count ? true : null;
        }
    }
}
