using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static void Round(this List<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return;

            point3Ds.ForEach(x => x.Round(tolerance));
        }
    }
}