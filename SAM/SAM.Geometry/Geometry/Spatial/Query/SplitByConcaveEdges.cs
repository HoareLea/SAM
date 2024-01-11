using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> SplitByConcaveEdges(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null)
                return null;

            throw new NotImplementedException();
        }
    }
}