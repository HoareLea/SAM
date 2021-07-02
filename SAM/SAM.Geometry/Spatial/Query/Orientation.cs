namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Orientation Orinetation(this IClosedPlanar3D closedPlanar3D)
        {
            Plane plane = closedPlanar3D.GetPlane();
            if(plane == null)
            {
                return Orientation.Undefined;
            }

            Planar.IClosed2D closed2D = plane.Convert(closedPlanar3D);
            if(closed2D == null)
            {
                return Orientation.Undefined;
            }

            return Planar.Query.Orientation(closed2D);
        }
    }
}