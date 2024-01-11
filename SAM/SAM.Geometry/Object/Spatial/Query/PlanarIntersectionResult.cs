using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Create
    {
        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, IFace3DObject face3DObject, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            return Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3DObject?.Face3D, tolerance_Angle, tolerance_Distance);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult<T>(Face3D face3D, IEnumerable<T> face3DObjects, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T: IFace3DObject
        {
            return Geometry.Spatial.Create.PlanarIntersectionResult(face3D, face3DObjects?.ToList().ConvertAll(x => x.Face3D), tolerance_Angle, tolerance_Distance);
        }
    }
}