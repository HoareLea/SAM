// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// One air movement the unit <b>delivers</b> - names the unit as its source and names a
        /// destination - or null where the unit delivers nothing.
        ///
        /// <para><b>Asked of the whole cluster, not of the unit's relations</b></para>
        /// <para>
        /// <see cref="AirFlow(AdjacencyCluster, AirHandlingUnitAirMovement, out Profile)"/> answers the
        /// same question over the movements RELATED to the unit, because that is all
        /// <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> can walk when it sizes the unit's intake. This
        /// answers it over every <see cref="SpaceAirMovement"/> in the model, from the
        /// <c>From</c>/<c>To</c> references on the object itself.
        /// </para>
        /// <para>
        /// The two differ exactly when a relation is missing, and that difference is a defect: the unit's
        /// generated TAS plant zone loses the air it delivers and gains no intake to replace it, which is a
        /// zone TAS refuses to simulate. <c>Create.Log</c> reports it.
        /// </para>
        /// <para>
        /// <b>The unit's own exhaust is not a delivery.</b> It names the unit as its source and names no
        /// destination - that is how "leaving the building" is said - so counting it would have the unit
        /// draw its own duty twice over. This is the same rule <c>AirFlow</c> applies, deliberately.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">The model.</param>
        /// <param name="airHandlingUnit">The unit.</param>
        public static SpaceAirMovement SpaceAirMovement_Delivered(this AdjacencyCluster adjacencyCluster, AirHandlingUnit airHandlingUnit)
        {
            if (adjacencyCluster == null || airHandlingUnit == null)
            {
                return null;
            }

            List<SpaceAirMovement> spaceAirMovements = adjacencyCluster.GetObjects<SpaceAirMovement>();
            if (spaceAirMovements == null || spaceAirMovements.Count == 0)
            {
                return null;
            }

            ObjectReference objectReference = new ObjectReference(airHandlingUnit);

            foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements)
            {
                if (spaceAirMovement == null || string.IsNullOrWhiteSpace(spaceAirMovement.From))
                {
                    continue;
                }

                //No destination is the unit's exhaust, which is air leaving rather than air delivered.
                if (string.IsNullOrWhiteSpace(spaceAirMovement.To))
                {
                    continue;
                }

                if (objectReference != Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.From))
                {
                    continue;
                }

                return spaceAirMovement;
            }

            return null;
        }
    }
}
