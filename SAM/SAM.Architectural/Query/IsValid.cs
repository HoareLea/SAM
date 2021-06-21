using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static bool IsValid(this HostBuildingElement hostBuildingElement, Opening opening, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D_HostBuildingElement = hostBuildingElement?.Face3D;
            if(face3D_HostBuildingElement == null)
            {
                return false;
            }

            Face3D face3D_Opening = opening?.Face3D;
            if(face3D_Opening == null)
            {
                return false;
            }

            Plane plane_HostBuildingElement = face3D_HostBuildingElement.GetPlane();
            if (plane_HostBuildingElement == null)
                return false;

            Plane plane_Opening = face3D_Opening.GetPlane();
            if (plane_Opening == null)
                return false;

            if (!plane_HostBuildingElement.Coplanar(plane_Opening, tolerance))
                return false;

            if (face3D_HostBuildingElement.Inside(face3D_Opening.InternalPoint3D(), tolerance))
                return true;

            return false;
        }
    }
}