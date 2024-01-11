namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool SelfIntersect(this Polygon3D polygon3D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon3D == null)
                return false;

            Plane plane = polygon3D.GetPlane();
            if (plane == null)
                return false;

            return Planar.Query.SelfIntersect(plane.Convert(polygon3D), tolerance);
        }
    }
}