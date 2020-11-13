namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Rectangular(this IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.Distance)
        {
            if (closedPlanar3D == null)
                return false;

            if (closedPlanar3D is Face3D)
                return Rectangular(((Face3D)closedPlanar3D).GetExternalEdge3D(), tolerance);

            if (!(closedPlanar3D is ISegmentable3D))
                return false;

            Plane plane = closedPlanar3D.GetPlane();

            Planar.IClosed2D closed2D = plane.Convert(closedPlanar3D);
            if (closed2D == null)
                return false;

            return Planar.Query.Rectangular(closed2D, tolerance);
        }
    }
}