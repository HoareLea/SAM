// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// The Part O assessment scope: which zones are dwellings and which are common space.
    /// <para>
    /// Modelled on the real three-flat structure the TAS validation run used - a <c>Flats</c> category
    /// holding Flat 1, Flat 2, Flat 3 and a Corridor - because that run showed the corridor being exported
    /// into the domestic overheating assessment as an ordinary room. The corridor does need assessing; it
    /// must simply never be part of a dwelling.
    /// </para>
    /// </summary>
    public class PartOAssessmentScopeTests
    {
        /// <summary>
        /// The three flats are dwellings and the corridor is not - and the corridor is still returned, as
        /// common space, rather than dropped. Losing it would lose an assessment the building needs.
        /// </summary>
        [Fact]
        public void ThreeFlatsAndACorridor_AreSplitIntoDwellingsAndCommonSpace()
        {
            Zones().PartOClassifyAssessmentZones(out List<Zone> zones_Dwelling, out List<Zone> zones_CommonSpace);

            Assert.Equal(["Flat 1", "Flat 2", "Flat 3"], zones_Dwelling.ConvertAll(x => x.Name));
            Assert.Equal(["Corridor"], zones_CommonSpace.ConvertAll(x => x.Name));
        }

        /// <summary>
        /// <b>Nothing is lost and nothing is counted twice.</b> Every zone of the category comes back in
        /// exactly one of the two lists - the property that makes "assessed separately" true rather than a
        /// hopeful description.
        /// </summary>
        [Fact]
        public void EveryZone_IsInExactlyOneScope()
        {
            List<Zone> zones = Zones();

            zones.PartOClassifyAssessmentZones(out List<Zone> zones_Dwelling, out List<Zone> zones_CommonSpace);

            Assert.Equal(zones.Count, zones_Dwelling.Count + zones_CommonSpace.Count);

            foreach (Zone zone in zones)
            {
                bool dwelling = zones_Dwelling.Find(x => x.Guid == zone.Guid) != null;
                bool commonSpace = zones_CommonSpace.Find(x => x.Guid == zone.Guid) != null;

                Assert.True(dwelling ^ commonSpace, string.Format("Zone '{0}' is in {1} scopes.", zone.Name, dwelling && commonSpace ? "both" : "neither"));
            }
        }

        /// <summary>
        /// The dwelling half is the Part F calculation's own selection, not a second opinion. Asserted
        /// against <c>Query.PartFDwellingZones</c> directly so the two cannot drift apart.
        /// </summary>
        [Fact]
        public void DwellingScope_IsThePartFSelection()
        {
            List<Zone> zones = Zones();

            zones.PartOClassifyAssessmentZones(out List<Zone> zones_Dwelling, out _);

            Assert.Equal(
                zones.PartFDwellingZones().ConvertAll(x => x.Guid),
                zones_Dwelling.ConvertAll(x => x.Guid));
        }

        /// <summary>
        /// A legacy category where no zone carries Is Dwelling is all dwellings and no common space - the
        /// same reading the Part F calculation takes, rather than this query inventing a corridor.
        /// </summary>
        [Fact]
        public void LegacyCategoryWithoutIsDwelling_IsAllDwellings()
        {
            List<Zone> zones = [Zone("Flat 1", null), Zone("Flat 2", null)];

            zones.PartOClassifyAssessmentZones(out List<Zone> zones_Dwelling, out List<Zone> zones_CommonSpace);

            Assert.Equal(2, zones_Dwelling.Count);
            Assert.Empty(zones_CommonSpace);
        }

        /// <summary>
        /// Two zones sharing a name are still told apart, because the split is by guid. A model with a
        /// "Corridor" on every floor is ordinary, and name matching is exactly how a common space ends up
        /// attributed to the wrong place.
        /// </summary>
        [Fact]
        public void ZonesSharingAName_AreToldApartByIdentity()
        {
            List<Zone> zones = [Zone("Corridor", true), Zone("Corridor", false)];

            zones.PartOClassifyAssessmentZones(out List<Zone> zones_Dwelling, out List<Zone> zones_CommonSpace);

            Assert.Single(zones_Dwelling);
            Assert.Single(zones_CommonSpace);
            Assert.NotEqual(zones_Dwelling[0].Guid, zones_CommonSpace[0].Guid);
        }

        [Fact]
        public void NoZones_IsNeitherScope()
        {
            ((List<Zone>)null).PartOClassifyAssessmentZones(out List<Zone> zones_Dwelling, out List<Zone> zones_CommonSpace);

            Assert.Empty(zones_Dwelling);
            Assert.Empty(zones_CommonSpace);
        }

        /// <summary>The structure of the model the TAS validation run used.</summary>
        private static List<Zone> Zones()
        {
            return [Zone("Flat 1", true), Zone("Corridor", false), Zone("Flat 2", true), Zone("Flat 3", true)];
        }

        private static Zone Zone(string name, bool? isDwelling)
        {
            Zone result = new(name);

            result.SetValue(ZoneParameter.ZoneCategory, "Flats");

            if (isDwelling != null)
            {
                result.SetValue(ZoneParameter.IsDwelling, isDwelling.Value);
            }

            return result;
        }
    }
}
