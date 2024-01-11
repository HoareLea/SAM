using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static bool Above(this Plane plane, IFace3DObject face3DObject, double tolerance = 0)
        {
            if (face3DObject == null || plane == null)
            {
                return false;
            }

            return Geometry.Spatial.Query.Above(plane, face3DObject.Face3D, tolerance);
        }
    }
}