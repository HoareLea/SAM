using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static bool Horizontal(this IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = face3DObject?.Face3D?.GetPlane();
            if (plane == null)
            {
                return false;
            }

            return Geometry.Spatial.Query.Horizontal(plane, tolerance);
        }
    }
}