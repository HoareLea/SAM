using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static List<Segment3D> Grid<T>(this IEnumerable<T> face3DObjects, double x, double y, Plane plane = null, Point3D point3D = null, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, bool keepFull = false) where T : IFace3DObject
        {
            if (face3DObjects == null || double.IsNaN(x) || double.IsNaN(y))
                return null;

            List<IClosedPlanar3D> closedPlanar3Ds = new List<IClosedPlanar3D>();
            foreach (T face3DObject in face3DObjects)
            {
                IClosedPlanar3D closedPlanar3D = face3DObject?.Face3D;
                if (closedPlanar3D == null)
                    continue;

                closedPlanar3Ds.Add(closedPlanar3D);
            }

            return Geometry.Spatial.Query.Grid(closedPlanar3Ds, x, y, plane, point3D, tolerance_Angle, tolerance_Distance, keepFull);
        }
    }
}