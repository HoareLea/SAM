using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double MinElevation(this Panel panel)
        {
            Geometry.Spatial.BoundingBox3D boundingBox3D = panel?.PlanarBoundary3D?.GetBoundingBox();
            if (boundingBox3D == null)
                return double.NaN;

            return boundingBox3D.Min.Z;
        }

        public static double MinElevation(this IEnumerable<Panel> panels)
        {
            if (panels == null || panels.Count() == 0)
                return double.NaN;

            double result = double.MaxValue;
            foreach(Panel panel in panels)
            {
                double minElevation = panel.MinElevation();
                if (double.IsNaN(minElevation))
                    continue;

                if (minElevation < result)
                    result = minElevation;
            }

            return result;
        }
    }
}