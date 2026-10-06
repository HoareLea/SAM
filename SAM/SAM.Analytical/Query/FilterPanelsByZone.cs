// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Finds Zones whose Zone Category Name (<see cref="ZoneParameter.ZoneCategory"/>) matches, case-insensitively, any of the given zoneCategoryNames.
        /// Zones sharing the same category name are returned as separate Zones (Zone identity is never merged by category).
        /// </summary>
        public static List<Zone> ZonesByCategoryName(this AdjacencyCluster adjacencyCluster, IEnumerable<string> zoneCategoryNames)
        {
            if (adjacencyCluster == null || zoneCategoryNames == null)
            {
                return null;
            }

            List<string> zoneCategoryNames_Trimmed = zoneCategoryNames.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();
            if (zoneCategoryNames_Trimmed.Count == 0)
            {
                return new List<Zone>();
            }

            List<Zone> zones = adjacencyCluster.GetZones();
            if (zones == null || zones.Count == 0)
            {
                return new List<Zone>();
            }

            List<Zone> result = new List<Zone>();
            foreach (Zone zone in zones)
            {
                if (zone == null || !zone.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory) || string.IsNullOrWhiteSpace(zoneCategory))
                {
                    continue;
                }

                string zoneCategory_Trimmed = zoneCategory.Trim();

                if (zoneCategoryNames_Trimmed.Any(x => x.Equals(zoneCategory_Trimmed, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(zone);
                }
            }

            return result;
        }

        /// <summary>
        /// Classifies Panels associated with the given Zones into external Panels, internal Panels between Spaces within the same Zone,
        /// and internal Panels between a Space in a Zone and a Space outside that same Zone. Zone identity (not Zone Category Name) drives the classification.
        /// </summary>
        /// <param name="adjacencyCluster">AdjacencyCluster containing the Zones, Spaces and Panels to query</param>
        /// <param name="zones">Zones to evaluate. Distinct Zones (by Guid) are treated individually even if they share a Zone Category Name</param>
        /// <param name="internalPanelsWithinZone">Internal Panels with at least two distinct adjacent Spaces, all belonging to the same one of the given Zones</param>
        /// <param name="internalPanelsToSpacesOutsideZone">Internal Panels where at least one adjacent Space belongs to a given Zone and at least one other adjacent Space does not belong to that same Zone</param>
        /// <returns>External Panels associated with at least one Space belonging to a given Zone</returns>
        public static List<Panel> FilterPanelsByZone(this AdjacencyCluster adjacencyCluster, IEnumerable<Zone> zones, out List<Panel> internalPanelsWithinZone, out List<Panel> internalPanelsToSpacesOutsideZone)
        {
            List<Panel> externalPanels = new List<Panel>();
            internalPanelsWithinZone = new List<Panel>();
            internalPanelsToSpacesOutsideZone = new List<Panel>();

            List<Zone> zones_Distinct = zones?.Where(x => x != null).GroupBy(x => x.Guid).Select(x => x.First()).ToList();
            if (adjacencyCluster == null || zones_Distinct == null || zones_Distinct.Count == 0)
            {
                return externalPanels;
            }

            // Space -> Zone membership, keyed by Zone identity (Guid), so Zones sharing a Category Name are never merged.
            Dictionary<Guid, HashSet<Guid>> spaceGuidsByZoneGuid = new Dictionary<Guid, HashSet<Guid>>();
            Dictionary<Guid, Space> space_ByGuid = new Dictionary<Guid, Space>();

            foreach (Zone zone in zones_Distinct)
            {
                HashSet<Guid> spaceGuids = new HashSet<Guid>();

                List<Space> spaces = adjacencyCluster.GetSpaces(zone);
                if (spaces != null)
                {
                    foreach (Space space in spaces)
                    {
                        if (space == null)
                        {
                            continue;
                        }

                        spaceGuids.Add(space.Guid);
                        if (!space_ByGuid.ContainsKey(space.Guid))
                        {
                            space_ByGuid[space.Guid] = space;
                        }
                    }
                }

                spaceGuidsByZoneGuid[zone.Guid] = spaceGuids;
            }

            if (space_ByGuid.Count == 0)
            {
                return externalPanels;
            }

            HashSet<Guid> externalPanelGuids = new HashSet<Guid>();
            HashSet<Guid> withinZonePanelGuids = new HashSet<Guid>();
            HashSet<Guid> outsideZonePanelGuids = new HashSet<Guid>();

            foreach (Space space in space_ByGuid.Values)
            {
                List<Panel> panels = adjacencyCluster.GetPanels(space);
                if (panels == null)
                {
                    continue;
                }

                foreach (Panel panel in panels)
                {
                    if (panel == null)
                    {
                        continue;
                    }

                    // Panel already classified (e.g. discovered from the other side of the same Panel, or shared between two matched Zones)
                    if (externalPanelGuids.Contains(panel.Guid) || withinZonePanelGuids.Contains(panel.Guid) || outsideZonePanelGuids.Contains(panel.Guid))
                    {
                        continue;
                    }

                    List<Space> panelSpaces = adjacencyCluster.GetSpaces(panel);
                    if (panelSpaces == null || panelSpaces.Count == 0)
                    {
                        // Orphan Panel with no adjacency information - not enough topology to classify, so it is omitted.
                        continue;
                    }

                    if (adjacencyCluster.External(panel))
                    {
                        externalPanelGuids.Add(panel.Guid);
                        externalPanels.Add(panel);
                        continue;
                    }

                    List<Guid> panelSpaceGuids = panelSpaces.Where(x => x != null).Select(x => x.Guid).Distinct().ToList();
                    if (panelSpaceGuids.Count < 2)
                    {
                        // Not enough distinct adjacent Spaces (e.g. a duplicate Space relation) to establish a within/outside-Zone relationship - omit.
                        continue;
                    }

                    bool touchesGivenZone = false;
                    bool withinAGivenZone = false;
                    foreach (Zone zone in zones_Distinct)
                    {
                        HashSet<Guid> zoneSpaceGuids = spaceGuidsByZoneGuid[zone.Guid];
                        if (!panelSpaceGuids.Any(x => zoneSpaceGuids.Contains(x)))
                        {
                            continue;
                        }

                        touchesGivenZone = true;

                        if (panelSpaceGuids.All(x => zoneSpaceGuids.Contains(x)))
                        {
                            withinAGivenZone = true;
                            break;
                        }
                    }

                    if (!touchesGivenZone)
                    {
                        continue;
                    }

                    if (withinAGivenZone)
                    {
                        withinZonePanelGuids.Add(panel.Guid);
                        internalPanelsWithinZone.Add(panel);
                    }
                    else
                    {
                        outsideZonePanelGuids.Add(panel.Guid);
                        internalPanelsToSpacesOutsideZone.Add(panel);
                    }
                }
            }

            return externalPanels;
        }
    }
}
