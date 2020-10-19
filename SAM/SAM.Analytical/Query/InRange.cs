using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> InRange(this IEnumerable<Panel> panels, Shell shell, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null || panels == null)
                return null;

            Dictionary<Face3D, Panel> dictionary = new Dictionary<Face3D, Panel>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null)
                    continue;

                dictionary[face3D] = panel;
            }

            List<Face3D> face3Ds = Geometry.Spatial.Query.InRange(shell, dictionary.Keys, tolerance);
            if (face3Ds == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Face3D face3D in face3Ds)
                result.Add(dictionary[face3D]);

            return result;
        }
    }
}