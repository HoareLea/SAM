using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static HostBuildingElementCategory HostBuildingElementCategory(Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return Architectural.HostBuildingElementCategory.Undefined;

            double value = normal.Unit.DotProduct(Vector3D.WorldZ);
            if (System.Math.Abs(value) <= tolerance)
                return Architectural.HostBuildingElementCategory.Wall;

            if (value < 0)
                return Architectural.HostBuildingElementCategory.Floor;

            return Architectural.HostBuildingElementCategory.Roof;
        }
    }
}