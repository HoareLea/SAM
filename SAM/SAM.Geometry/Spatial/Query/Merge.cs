using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Shell Merge(this Shell shell, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> face3Ds = shell?.Face3Ds;
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            return new Shell(Union(face3Ds, tolerance));
        }
    }
}