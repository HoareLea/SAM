namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D Location(this IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
                return null;

            Plane plane = closedPlanar3D.GetPlane();
            if (plane == null)
                return null;

            return plane.Origin;
        }
    }
}