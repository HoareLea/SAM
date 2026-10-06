// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The zone categories in this model that a Part F calculation would find dwellings in, in name order.
        /// <para>
        /// Answers "which category holds the flats" without anybody having to hard-code "Flats". A caller that
        /// gets exactly one name can use it; more than one is genuinely ambiguous and is the user's choice to
        /// make; none means the model has no dwelling-zone structure at all, and the calculation's whole-house
        /// mode applies.
        /// </para>
        /// <para>
        /// <b>It asks the calculator's own rule rather than restating it.</b> The decision about which zones in
        /// a category are dwellings lives in one place, <see cref="PartFDwellingZones"/>, which is what
        /// <c>PartFCalculator</c> selects with too - so a category this returns really does size at least one
        /// dwelling, and it cannot come to disagree with the calculation.
        /// </para>
        /// </summary>
        public static List<string> PartFDwellingZoneCategories(this AdjacencyCluster adjacencyCluster)
        {
            List<string> result = [];

            List<Zone> zones = adjacencyCluster?.GetZones();
            if (zones == null)
            {
                return result;
            }

            Dictionary<string, List<Zone>> dictionary = [];

            foreach (Zone zone in zones)
            {
                string name = zone?.GetValue<string>(ZoneParameter.ZoneCategory);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (!dictionary.TryGetValue(name, out List<Zone> zones_Category))
                {
                    zones_Category = [];
                    dictionary[name] = zones_Category;
                }

                zones_Category.Add(zone);
            }

            foreach (KeyValuePair<string, List<Zone>> keyValuePair in dictionary)
            {
                //The one rule, asked rather than repeated.
                if (keyValuePair.Value.PartFDwellingZones().Count != 0)
                {
                    result.Add(keyValuePair.Key);
                }
            }

            result.Sort(System.StringComparer.Ordinal);

            return result;
        }
    }
}
