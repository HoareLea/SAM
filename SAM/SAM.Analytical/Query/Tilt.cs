using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Tilt(this Panel panel)
        {
            if (panel == null)
                return double.NaN;

            return Geometry.Spatial.Query.Tilt(panel.GetFace3D());
        }
    }
}