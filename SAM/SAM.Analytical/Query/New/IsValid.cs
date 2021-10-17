using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool IsValid(this IHostPartition hostPartition, IOpening opening, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D_hostPartition = hostPartition?.Face3D;
            if(face3D_hostPartition == null)
            {
                return false;
            }

            Face3D face3D_Opening = opening?.Face3D;
            if(face3D_Opening == null)
            {
                return false;
            }

            Plane plane_hostPartition = face3D_hostPartition.GetPlane();
            if (plane_hostPartition == null)
                return false;

            Plane plane_Opening = face3D_Opening.GetPlane();
            if (plane_Opening == null)
                return false;

            if (!plane_hostPartition.Coplanar(plane_Opening, tolerance))
                return false;

            if (face3D_hostPartition.Inside(face3D_Opening.InternalPoint3D(), tolerance))
                return true;

            return false;
        }
    }
}