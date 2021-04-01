using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Face2D Join(this Face2D face2D, IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null)
                return null;

            if (face2Ds == null || face2Ds.Count() == 0)
                return new Face2D(face2D);

            Point2D point2D = face2D.GetInternalPoint2D();
            if (point2D == null)
                return new Face2D(face2D);

            List<Face2D> face2Ds_Temp = new List<Face2D>(face2Ds);
            face2Ds_Temp.Add(face2D);

            face2Ds_Temp = face2Ds_Temp.Union(tolerance);
            if (face2Ds_Temp == null || face2Ds_Temp.Count == 0)
                return new Face2D(face2D);

            Face2D result = face2Ds_Temp.Find(x => x.Inside(point2D, tolerance));
            if (result == null)
                return new Face2D(face2D);

            return result;
        }
    }
}