using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double MaxElevation(this Panel panel)
        {
            Geometry.Spatial.BoundingBox3D boundingBox3D = panel?.PlanarBoundary3D?.GetBoundingBox();
            if (boundingBox3D == null)
                return double.NaN;

            return boundingBox3D.Max.Z;
        }

        public static double MaxElevation(this IEnumerable<Panel> panels)
        {
            if (panels == null || panels.Count() == 0)
                return double.NaN;
            
            double result = double.MinValue;
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                double maxElevation = MaxElevation(panel);
                if (maxElevation > result)
                    result = maxElevation;
            }

            return result;
        }
    }
}