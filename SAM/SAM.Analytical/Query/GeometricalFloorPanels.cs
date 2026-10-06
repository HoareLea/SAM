// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// The space's floor panels identified purely by geometry: those whose space-outward normal points
        /// downwards within <paramref name="maxTiltDifference"/> of straight down.
        /// </summary>
        /// <remarks>
        /// Classification is space-relative (via
        /// <see cref="NormalDictionary(AdjacencyCluster, ISpace, out Shell, bool, double, double)"/>), not based
        /// on the panel's own stored face orientation or <see cref="Panel.PanelType"/>, so it still works on
        /// imported geometry with incomplete metadata and on virtual <see cref="PanelType.Air"/> boundaries.
        /// Callers that need a floor AREA should use
        /// <see cref="FloorArea(AdjacencyCluster, Space, out FloorAreaCalculationMethod, double, double, double, double)"/>,
        /// which adds panel-type protection on top of this geometric test.
        /// </remarks>
        /// <param name="adjacencyCluster">Adjacency cluster the space belongs to.</param>
        /// <param name="space">Space whose floor panels are wanted.</param>
        /// <param name="maxTiltDifference">
        /// Maximum deviation from straight down, in degrees. Tilt is measured from world Z, so a horizontal
        /// floor has tilt 180 and the accepted band is [180 - maxTiltDifference, 180 + maxTiltDifference]. The
        /// default 20 therefore accepts a floor ramped up to 20 degrees from horizontal, and rejects steeper
        /// downward-facing surfaces, which are sloped walls rather than occupied floor.
        /// </param>
        /// <param name="silverSpacing">Snap/sliver tolerance.</param>
        /// <param name="tolerance">Distance tolerance.</param>
        public static List<Panel> GeomericalFloorPanels(this AdjacencyCluster adjacencyCluster, Space space, double maxTiltDifference = 20, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || space == null)
                return null;

            Dictionary<IPanel, Vector3D> dictionary = adjacencyCluster.NormalDictionary(space, out Shell shell, true, silverSpacing, tolerance);
            if (dictionary == null || dictionary.Count == 0)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (KeyValuePair<IPanel, Vector3D> keyValuePair in dictionary)
            {
                Vector3D vector3D = keyValuePair.Value;
                if (vector3D == null)
                    continue;

                Panel panel = keyValuePair.Key as Panel;
                if (panel == null)
                    continue;

                if (Vector3D.WorldZ.SameHalf(vector3D))
                    continue;

                double tilt = Geometry.Spatial.Query.Tilt(vector3D);
                if (double.IsNaN(tilt))
                    continue;

                if (180 - maxTiltDifference > tilt || tilt > 180 + maxTiltDifference)
                    continue;

                result.Add(panel);
            }

            return result;
        }
    }
}
