using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Panel, List<T>> SectionDictionary<T>(this IEnumerable<Panel> panels, Plane plane, double tolerance = Core.Tolerance.Distance) where T: ISAMGeometry3D
        {
            if (plane == null || panels == null)
                return null;

            Dictionary<Panel, List<T>> result = new Dictionary<Panel, List<T>>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if(face3D == null)
                {
                    continue;
                }

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance);
                if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                result[panel] = planarIntersectionResult.GetGeometry3Ds<T>();
            }

            return result;
        }
    }
}