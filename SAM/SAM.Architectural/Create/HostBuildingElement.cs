using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static HostBuildingElement HostBuildingElement(this Face3D face3D, HostBuildingElementType hostBuildingElementType = null, double tolerance = Core.Tolerance.Angle)
        {
            if(face3D == null || !face3D.IsValid())
            {
                return null;
            }

            if(hostBuildingElementType == null)
            {
                hostBuildingElementType = Query.DefaultHostBuildingElementType(face3D, tolerance);
            }

            if (hostBuildingElementType is WallType)
            {
                return new Wall((WallType)hostBuildingElementType, face3D);
            }

            if(hostBuildingElementType is RoofType)
            {
                return new Roof((RoofType)hostBuildingElementType, face3D);
            }

            if(hostBuildingElementType is FloorType)
            {
                return new Floor((FloorType)hostBuildingElementType, face3D);
            }

            return null;
        }
    }
}
