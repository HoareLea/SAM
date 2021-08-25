using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Face2D> Fill(this Face2D face2D, IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null || face2Ds == null)
            {
                return null;
            }

            throw new System.NotImplementedException();
        }
    }
}