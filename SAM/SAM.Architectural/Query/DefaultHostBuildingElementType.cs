using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static HostBuildingElementType DefaultHostBuildingElementType(Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return null;
            HostBuildingElementCategory hostBuildingElementCategory = Query.HostBuildingElementCategory(normal, tolerance);
            switch (hostBuildingElementCategory)
            {
                case Architectural.HostBuildingElementCategory.Floor:
                    return new FloorType(string.Empty);

                case Architectural.HostBuildingElementCategory.Roof:
                    return new RoofType(string.Empty);

                case Architectural.HostBuildingElementCategory.Wall:
                    return new WallType(string.Empty);

                default:
                    return null;
            }
        }

        public static HostBuildingElementType DefaultHostBuildingElementType(Face3D face3D, double tolerance = Core.Tolerance.Angle)
        {
            return DefaultHostBuildingElementType(face3D?.GetPlane()?.Normal, tolerance);
        }
    }
}