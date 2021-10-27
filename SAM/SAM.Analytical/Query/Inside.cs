using SAM.Geometry.Spatial;
using System.Collections.Generic;

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

        public static List<Space> Inside(IEnumerable<Space> spaces, Shell shell, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(spaces == null || shell == null)
            {
                return null;
            }

            List<Space> result = new List<Space>();
            foreach(Space space in spaces)
            {
                Point3D point3D = space?.Location;
                if(point3D == null)
                {
                    continue;
                }

                if(shell.Inside(point3D, silverSpacing, tolerance))
                {
                    result.Add(space);
                }
            }

            return result;
        }
    }
}