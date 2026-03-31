// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void ReplaceNameSpecialCharacters(this AdjacencyCluster adjacencyCluster, string name)
        {
            if (adjacencyCluster == null)
            {
                return;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels != null)
            {
                foreach (Panel panel in panels)
                {
                    Panel panel_New = Query.ReplaceNameSpecialCharacters(panel, name);

                    if (panel_New != null && panel_New.HasApertures)
                    {
                        foreach (Aperture aperture in panel_New.Apertures)
                        {
                            Aperture aperture_New = Query.ReplaceNameSpecialCharacters(aperture, name);
                            if (aperture_New == aperture)
                                continue;

                            panel_New = Create.Panel(panel_New);

                            panel_New.RemoveAperture(aperture.Guid);
                            panel_New.AddAperture(aperture_New);
                        }
                    }

                    if (panel_New != panel)
                        adjacencyCluster.AddObject(panel_New);
                }
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces != null)
            {
                foreach (Space space in spaces)
                {
                    if (space?.InternalCondition == null)
                    {
                        continue;
                    }

                    InternalCondition internalCondition = Query.ReplaceNameSpecialCharacters(space.InternalCondition, name);
                    Space space_New = null;
                    if (internalCondition.Name != space.InternalCondition.Name)
                        space_New = new Space(space) { InternalCondition = internalCondition };
                    else
                        space_New = space;

                    space_New = Query.ReplaceNameSpecialCharacters(space_New, name);
                    if (space_New != space)
                        adjacencyCluster.AddObject(space_New);
                }
            }
        }
    }
}
