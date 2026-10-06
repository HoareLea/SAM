// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Stores the canonical floor area on every space in the cluster.
        /// </summary>
        /// <remarks>
        /// This is the single creation-time hook: <see cref="Create"/>'s adjacency cluster paths and
        /// SAM_OCCT's OCCT paths all call it once their panels, relations, normals and panel types are final,
        /// so no path carries its own definition of floor area. The value comes from
        /// <see cref="Query.FloorArea(AdjacencyCluster, Space, out FloorAreaCalculationMethod, double, double, double, double)"/>,
        /// which follows a ramped floor's actual surface rather than its projection, and which accepts a
        /// geometrically valid <see cref="PanelType.Air"/> lower boundary as a virtual floor without retyping it.
        ///
        /// A space is written only when a calculation succeeded. When neither the geometrical panels nor the
        /// shell section produced a finite positive value, any existing valid area is left exactly as it was
        /// and nothing is stored - never 0, NaN or infinity.
        /// </remarks>
        /// <param name="adjacencyCluster">Adjacency cluster to update in place.</param>
        /// <param name="maxTiltDifference">Maximum deviation, in degrees, from straight down that still counts as floor. The default 20 accepts floors ramped up to 20 degrees from horizontal.</param>
        /// <param name="silverSpacing">Snap/sliver tolerance.</param>
        /// <param name="tolerance_Angle">Angle tolerance.</param>
        /// <param name="tolerance_Distance">Distance tolerance.</param>
        /// <returns>Number of spaces whose stored area was recalculated.</returns>
        public static int UpdateFloorAreas(this AdjacencyCluster adjacencyCluster, double maxTiltDifference = 20, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            List<Space> spaces = adjacencyCluster?.GetSpaces();
            if (spaces == null || spaces.Count == 0)
            {
                return 0;
            }

            int result = 0;
            foreach (Space space in spaces)
            {
                if (space == null)
                {
                    continue;
                }

                double area = adjacencyCluster.FloorArea(space, out FloorAreaCalculationMethod floorAreaCalculationMethod, maxTiltDifference, silverSpacing, tolerance_Angle, tolerance_Distance);

                // Existing/Undefined mean nothing was recalculated: leave the stored parameter untouched so a
                // valid pre-existing area survives incomplete or degenerate geometry.
                if (floorAreaCalculationMethod != FloorAreaCalculationMethod.GeometricalFloorPanels && floorAreaCalculationMethod != FloorAreaCalculationMethod.HorizontalSection)
                {
                    continue;
                }

                Space space_Temp = new Space(space);
                space_Temp.SetValue(SpaceParameter.Area, area);
                adjacencyCluster.AddObject(space_Temp);
                result++;
            }

            return result;
        }
    }
}
