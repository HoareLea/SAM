// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Air movements below this net [m3/s] at a node are treated as balanced. A ten-thousandth of a
        /// litre per second - far below anything a design states, and far above the rounding a
        /// proportional allocation and an l/s to m3/s division leave behind.
        /// </summary>
        public const double AirMovementResidualTolerance = 1e-9;

        /// <summary>
        /// The net air movement [m3/s] at every node a set of <see cref="SpaceAirMovement"/>s touches:
        /// what arrives, less what leaves. Zero is a balanced node.
        ///
        /// <para><b>Why this is summed per node rather than matched per route.</b></para>
        /// <para>
        /// The air movements of a dwelling form a directed network, not a set of end-to-end journeys. One
        /// unit feeds several rooms, one room draws from several rooms and passes air on to several more,
        /// and flows split and recombine along the way - a bedroom routinely divides its supply between
        /// three rooms while a wet room draws from two. Nothing in that has a matching partner, and a check that looked
        /// for one would reject a correct model. What conservation actually requires, and what TAS
        /// actually enforces, is only that the sums agree at each node.
        /// </para>
        ///
        /// <para><b>Why the unit's outside intake is counted here.</b></para>
        /// <para>
        /// An air handling unit's intake is not carried as an object: <c>SAM.Analytical.Tas</c> derives it
        /// from what the unit delivers, so the unit's TAS zone receives exactly the sum of the movements
        /// leaving it towards a destination. Leaving it out would report every unit as unbalanced by its
        /// own supply duty, so it is added on the same terms the writer uses. An outward movement - one
        /// with no destination - is not part of the intake and is not counted into it.
        /// </para>
        /// </summary>
        /// <param name="spaceAirMovements">The movements to sum. Null entries are skipped.</param>
        /// <param name="airHandlingUnits">
        /// The units whose outside intake should be counted. Null or empty leaves every unit reported as
        /// carrying the raw net of the movements alone, which is what a caller checking spaces only wants.
        /// </param>
        /// <returns>Net [m3/s] by node guid. A node no movement touches does not appear.</returns>
        public static Dictionary<Guid, double> AirMovementResidual(this AdjacencyCluster adjacencyCluster, IEnumerable<SpaceAirMovement> spaceAirMovements, IEnumerable<AirHandlingUnit> airHandlingUnits)
        {
            Dictionary<Guid, double> result = [];

            if (adjacencyCluster is null || spaceAirMovements is null)
            {
                return result;
            }

            void Add(SAMObject sAMObject, double value)
            {
                if (sAMObject is null)
                {
                    return;
                }

                result[sAMObject.Guid] = (result.TryGetValue(sAMObject.Guid, out double existing) ? existing : 0) + value;
            }

            List<SpaceAirMovement> spaceAirMovements_Temp = [];

            foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements)
            {
                if (spaceAirMovement is null || double.IsNaN(spaceAirMovement.AirFlow))
                {
                    continue;
                }

                spaceAirMovements_Temp.Add(spaceAirMovement);

                Add(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.From), -spaceAirMovement.AirFlow);
                Add(adjacencyCluster.AirMovementEndpoint(spaceAirMovement.To), spaceAirMovement.AirFlow);
            }

            foreach (AirHandlingUnit airHandlingUnit in airHandlingUnits ?? [])
            {
                if (airHandlingUnit is null)
                {
                    continue;
                }

                ObjectReference objectReference = new(airHandlingUnit);

                double intake = 0;

                foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements_Temp)
                {
                    //Only what the unit delivers somewhere. Its outward movement carries the extract air
                    //back out of the building and is not drawn in again.
                    if (string.IsNullOrWhiteSpace(spaceAirMovement.To))
                    {
                        continue;
                    }

                    if (objectReference == Core.Convert.ComplexReference<ObjectReference>(spaceAirMovement.From))
                    {
                        intake += spaceAirMovement.AirFlow;
                    }
                }

                if (intake != 0)
                {
                    Add(airHandlingUnit, intake);
                }
            }

            return result;
        }

        /// <summary>
        /// The model object one end of an air movement names, or <b>null</b> where it names nothing - which
        /// is how outside is said.
        /// </summary>
        public static SAMObject AirMovementEndpoint(this AdjacencyCluster adjacencyCluster, string reference)
        {
            if (adjacencyCluster is null || string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            ObjectReference objectReference = Core.Convert.ComplexReference<ObjectReference>(reference);
            if (objectReference is null)
            {
                return null;
            }

            List<SAMObject> sAMObjects = adjacencyCluster.GetObjects<SAMObject>(objectReference);

            return sAMObjects is null || sAMObjects.Count == 0 ? null : sAMObjects[0];
        }
    }
}
