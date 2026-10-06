// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Writes the Approved Document F transfer air record onto a door aperture in the model.
        /// <para>
        /// <see cref="Panel.Apertures"/> hands out CLONES, so setting a parameter on an aperture read from
        /// it changes a copy and nothing else. The aperture therefore has to be removed from its panel and
        /// re-added, and the panel put back into the cluster, for the value to survive. This one method
        /// exists so that every caller - the calculator, SAM_UI, Grasshopper and the tests - goes through
        /// the same correct path rather than each rediscovering the clone.
        /// </para>
        /// </summary>
        /// <returns>True where the aperture was found and updated.</returns>
        public static bool SetPartFDoorTransferData(this AdjacencyCluster adjacencyCluster, Guid guid_Aperture, PartFDoorTransferData partFDoorTransferData)
        {
            if (adjacencyCluster is null || guid_Aperture == Guid.Empty || partFDoorTransferData is null)
            {
                return false;
            }

            foreach (Panel panel in adjacencyCluster.GetPanels() ?? [])
            {
                //GetAperture returns the panel's own instance, but modifying it is still not enough on its
                //own: the panel has to go back into the cluster, and the cluster stores panels by value.
                Aperture aperture = panel?.GetAperture(guid_Aperture);
                if (aperture is null)
                {
                    continue;
                }

                Aperture aperture_Updated = new(aperture);
                aperture_Updated.SetValue(ApertureParameter.PartFDoorTransferData, partFDoorTransferData);

                if (!panel.RemoveAperture(guid_Aperture))
                {
                    return false;
                }

                if (!panel.AddAperture(aperture_Updated))
                {
                    //Put the original back rather than leaving the panel with one fewer aperture than it
                    //started with. A failed write must not quietly delete a door.
                    panel.AddAperture(aperture);
                    return false;
                }

                adjacencyCluster.AddObject(panel);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads every Approved Document F transfer air record already on the model's door apertures,
        /// keyed by aperture. Used to carry the engineer's inputs across a recalculation.
        /// </summary>
        public static Dictionary<Guid, PartFDoorTransferData> GetPartFDoorTransferData(this AdjacencyCluster adjacencyCluster)
        {
            Dictionary<Guid, PartFDoorTransferData> result = [];

            foreach (Panel panel in adjacencyCluster?.GetPanels() ?? [])
            {
                foreach (Aperture aperture in panel?.Apertures ?? [])
                {
                    if (aperture?.GetValue<PartFDoorTransferData>(ApertureParameter.PartFDoorTransferData) is PartFDoorTransferData partFDoorTransferData)
                    {
                        result[aperture.Guid] = partFDoorTransferData;
                    }
                }
            }

            return result;
        }
    }
}
