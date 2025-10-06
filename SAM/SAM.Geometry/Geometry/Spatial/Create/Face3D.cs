namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Face3D Face3D(this Segment3D segment3D, double height)
        {
            if (segment3D == null || height == 0)
            {
                return null;
            }

            Polygon3D polygon3D = Polygon3D(segment3D, height);
            if (polygon3D == null)
            {
                return null;
            }

            return new Face3D(polygon3D);
        }

        public static Face3D Face3D(this Segment3D segment3D, Vector3D vector3D)
        {
            if (segment3D == null || vector3D == null)
            {
                return null;
            }

            Polygon3D polygon3D = Polygon3D(segment3D, vector3D);
            if (polygon3D == null)
            {
                return null;
            }

            return new Face3D(polygon3D);
        }

        public static Face3D Face3D(this IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
            {
                return null;
            }

            if(closedPlanar3D is Face3D)
            {
                return new Face3D((Face3D)closedPlanar3D);
            }

            return new Face3D(closedPlanar3D);
        }

        public static Face3D Face3D(this Plane plane, Planar.IClosed2D closed2D)
        {
            if (closed2D == null || plane == null)
            {
                return null;
            }

            return new Face3D(plane, closed2D);
        }
    }
}