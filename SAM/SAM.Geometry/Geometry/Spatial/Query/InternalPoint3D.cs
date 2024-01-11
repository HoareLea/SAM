namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D InternalPoint3D(this IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = closedPlanar3D?.GetPlane();
            if (plane == null)
                return null;

            Planar.Point2D point2D = closedPlanar3D.InternalPoint2D(tolerance);
            if (point2D == null)
                return null;

            return plane.Convert(point2D);

        }
    }
}