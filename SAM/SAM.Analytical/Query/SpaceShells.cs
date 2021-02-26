using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Shell> SpaceShells(this IEnumerable<Shell> shells, Point3D point3D, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (shells == null || point3D == null)
                return null;

            Point3D point3D_Moved = (Point3D)point3D.GetMoved(new Vector3D(0, 0, silverSpacing));

            List<Tuple<bool, bool, Shell>> tuples = new List<Tuple<bool, bool, Shell>>();
            foreach (Shell shell in shells)
            {
                if (shell == null)
                    continue;

                BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();
                if (boundingBox3D_Shell == null)
                    continue;

                bool inside_1 = shell.Inside(point3D, silverSpacing, tolerance);
                bool inside_2 = shell.Inside(point3D_Moved, silverSpacing, tolerance);

                if (!inside_1 && !inside_2)
                    continue;

                tuples.Add(new Tuple<bool, bool, Shell>(inside_1, inside_2, shell));
            }

            List<Shell> result = new List<Shell>();
            if (tuples == null || tuples.Count == 0)
                return result;

            foreach (Tuple<bool, bool, Shell> tuple in tuples)
            {
                if (tuple.Item1 && tuple.Item2)
                    result.Add(tuple.Item3);
            }

            if (result != null && tuples.Count > 0)
                return result;

            foreach (Tuple<bool, bool, Shell> tuple in tuples)
            {
                if (tuple.Item1)
                    result.Add(tuple.Item3);
            }

            if (result != null && tuples.Count > 0)
                return result;

            return tuples.ConvertAll(x => x.Item3);
        }
    }
}