// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The space's stored <see cref="SpaceParameter.Area"/> when it has one, otherwise the canonical floor
        /// area derived from the cluster.
        /// </summary>
        /// <remarks>
        /// The derivation is delegated to
        /// <see cref="FloorArea(AdjacencyCluster, Space, out FloorAreaCalculationMethod, double, double, double, double)"/>
        /// so this query cannot disagree with creation time. It used to prefer a horizontal shell section over
        /// the floor panels, which reported a ramp's plan area instead of its walking surface.
        /// </remarks>
        public static double CalculatedArea(this Space space, AdjacencyCluster adjacencyCluster = null)
        {
            if (space == null)
            {
                return double.NaN;
            }

            // An already-stored value still wins here: callers use this as a cheap read of what the model says,
            // not as a recalculation. Modify.UpdateAreaAndVolume removes the parameter first when it wants one.
            if (space.TryGetValue(SpaceParameter.Area, out double result) && !double.IsNaN(result))
            {
                return result;
            }

            if (adjacencyCluster == null)
            {
                return double.NaN;
            }

            return FloorArea(adjacencyCluster, space, out FloorAreaCalculationMethod floorAreaCalculationMethod);
        }
    }
}
