// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Splits a zone category into what Approved Document O assesses as <b>dwellings</b> and what it
        /// assesses as <b>common space</b> - a communal corridor, a stair, a landlord area.
        /// <para>
        /// <b>Both are assessed. Neither is discarded.</b> That is the whole point of this query, and it is
        /// why it is not simply "the Part F dwelling zones". A communal corridor is excluded from a dwelling
        /// because it belongs to no dwelling, not because nothing needs to be said about it: TM59 has a
        /// corridor criterion of its own, and dropping the corridor would silently lose an assessment the
        /// building needs. What must never happen is the corridor being attributed to Flat 1, Flat 2 or
        /// Flat 3.
        /// </para>
        /// <para>
        /// <b>The dwelling half is not decided here.</b> It is delegated to
        /// <see cref="PartFDwellingZones(IEnumerable{Zone})"/>, which remains the single source of truth for
        /// what a dwelling is - so a zone this calls a dwelling is exactly a zone the Part F calculation
        /// sizes, and the two can never drift apart. This adds one thing only: a name for everything else in
        /// the category.
        /// </para>
        /// <para>
        /// Deliberately not a general scope framework. Two outputs, one rule, no configuration - Part O needs
        /// to tell a flat from a corridor and nothing more than that yet.
        /// </para>
        /// </summary>
        /// <param name="zones">
        /// The zones of one category - typically <c>"Flats"</c>, holding both the flats and the corridor.
        /// </param>
        /// <param name="zones_Dwelling">
        /// The zones assessed as dwellings, exactly as the Part F calculation selects them.
        /// </param>
        /// <param name="zones_CommonSpace">
        /// Every other zone in the set: assessed in its own right, attributed to no dwelling.
        /// </param>
        public static void PartOClassifyAssessmentZones(this IEnumerable<Zone> zones, out List<Zone> zones_Dwelling, out List<Zone> zones_CommonSpace)
        {
            //The one rule, asked rather than repeated.
            zones_Dwelling = PartFDwellingZones(zones);

            zones_CommonSpace = [];

            foreach (Zone zone in zones ?? [])
            {
                if (zone == null)
                {
                    continue;
                }

                //Identity, not name: two zones can be called the same thing.
                if (zones_Dwelling.Find(x => x != null && x.Guid == zone.Guid) != null)
                {
                    continue;
                }

                zones_CommonSpace.Add(zone);
            }
        }
    }
}
