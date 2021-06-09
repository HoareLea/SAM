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

        public static List<Panel> Inside(this IEnumerable<Panel> panels, Shell shell, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null || panels == null)
                return null;

            Dictionary<Face3D, Panel> dictionary = new Dictionary<Face3D, Panel>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                dictionary[face3D] = panel;
            }

            List<Face3D> face3Ds = Geometry.Spatial.Query.Inside(shell, dictionary.Keys, silverSpacing, tolerance);
            if (face3Ds == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Face3D face3D in face3Ds)
                result.Add(dictionary[face3D]);

            return result;
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