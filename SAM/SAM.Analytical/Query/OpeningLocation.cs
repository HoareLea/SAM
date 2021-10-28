using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Point3D OpeningLocation(this IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.Distance)
        {
            if(closedPlanar3D == null)
            {
                return null;
            }

            Plane plane = closedPlanar3D.GetPlane();

            Point3D result = closedPlanar3D.GetCentroid();
            if (Geometry.Spatial.Query.Vertical(plane, tolerance))
            {
                result = new Point3D(result.X, result.Y, closedPlanar3D.GetBoundingBox().Min.Z);
            }

            return result;
        }
    }
}