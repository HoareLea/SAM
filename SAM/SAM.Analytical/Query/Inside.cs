using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Inside(this AdjacencyCluster adjacencyCluster, Space space, Point3D point3D, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || space == null || point3D == null)
                return false;

            Shell shell = adjacencyCluster.Shell(space);
            if (shell == null)
                return false;

            return shell.Inside(point3D, silverSpacing, tolerance);
        }
    }
}