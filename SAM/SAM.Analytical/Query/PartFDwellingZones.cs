// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Which of these zones a Part F calculation treats as dwellings. <b>The single source of the
        /// dwelling-selection policy</b> - <c>PartFCalculator</c> decides with this, and so does anything that
        /// needs to know what the calculator would do without running it.
        /// <para>
        /// The policy, and why it is not simply "IsDwelling is true":
        /// </para>
        /// <list type="bullet">
        /// <item>where NO zone carries <see cref="ZoneParameter.IsDwelling"/>, every zone is a dwelling. The
        /// parameter postdates the models, so reading its absence as "not a dwelling" would make a model built
        /// before it existed size nothing at all.</item>
        /// <item>otherwise only an explicit <c>true</c> is a dwelling. An unmarked zone beside marked ones is
        /// NOT sized - the model is telling us something about its zones and staying silent about that one -
        /// and neither is an explicit <c>false</c>, which is how a shared corridor, a landlord area or a
        /// commercial unit is kept out of a dwelling calculation.</item>
        /// </list>
        /// <para>
        /// Pure: it reports nothing and warns about nothing. The calculator's warnings and remarks about
        /// excluded and unmarked zones are its own business and stay there - see
        /// <see cref="PartFClassifyDwellingZones"/>, which gives it the lists to report on.
        /// </para>
        /// </summary>
        public static List<Zone> PartFDwellingZones(this IEnumerable<Zone> zones)
        {
            PartFClassifyDwellingZones(zones, out List<Zone> zones_Dwelling, out List<Zone> zones_NotDwelling, out List<Zone> zones_Unmarked);

            //Nothing in this set says anything about being a dwelling: legacy model, all of them count.
            return zones_Dwelling.Count == 0 && zones_NotDwelling.Count == 0 ? zones_Unmarked : zones_Dwelling;
        }

        /// <summary>
        /// Sorts zones by what they say about <see cref="ZoneParameter.IsDwelling"/>: explicitly yes,
        /// explicitly no, and silent. The classification only - the policy that turns it into a selection is
        /// <see cref="PartFDwellingZones"/>, and the reporting is the calculator's.
        /// <para>
        /// The three-way split matters and a two-way one will not do: <c>TryGetValue</c> distinguishes
        /// "explicitly false" from "no value at all", which <c>GetValue</c> cannot - both come back as false.
        /// </para>
        /// </summary>
        public static void PartFClassifyDwellingZones(this IEnumerable<Zone> zones, out List<Zone> zones_Dwelling, out List<Zone> zones_NotDwelling, out List<Zone> zones_Unmarked)
        {
            zones_Dwelling = [];
            zones_NotDwelling = [];
            zones_Unmarked = [];

            foreach (Zone zone in zones ?? [])
            {
                if (zone == null)
                {
                    continue;
                }

                if (!zone.TryGetValue(ZoneParameter.IsDwelling, out bool isDwelling))
                {
                    zones_Unmarked.Add(zone);
                    continue;
                }

                (isDwelling ? zones_Dwelling : zones_NotDwelling).Add(zone);
            }
        }
    }
}
