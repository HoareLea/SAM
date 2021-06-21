namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static IClosedPlanar3D IClosedPlanar3D(this IClosedPlanar3D closedPlanar3D, Point3D point3D)
        {
            if (point3D == null)
            {
                return null;
            }

            Plane plane = closedPlanar3D?.GetPlane();
            if (plane == null)
            {
                return null;
            }

            plane = new Plane(plane, plane.Project(point3D));

            Planar.IClosed2D closed2D = plane.Convert(closedPlanar3D);

            return plane.Convert(closed2D);
        }

    }
}