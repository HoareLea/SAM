namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Face3D Face3D(this Segment3D segment3D, double height, double tolerance = Core.Tolerance.Angle)
        {
            if (segment3D == null || height == 0)
                return null;

            Polygon3D polygon3D = Polygon3D(segment3D, height);
            if (polygon3D == null)
                return null;

            return new Face3D(polygon3D);
        }

        public static Face3D Face3D(this IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
                return null;

            if(closedPlanar3D is Face3D)
            {
                return new Face3D((Face3D)closedPlanar3D);
            }

            return new Face3D(closedPlanar3D);
        }

        public static Face3D Face3D(this Plane plane, Planar.IClosed2D closed2D)
        {
            if (closed2D == null || plane == null)
                return null;

            return new Face3D(plane, closed2D);
        }
    }
}